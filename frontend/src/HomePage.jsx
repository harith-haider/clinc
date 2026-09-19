import React, { Suspense, useRef, useState, useEffect } from 'react';
import { useNavigate } from 'react-router-dom';
import { Canvas } from "@react-three/fiber";
import { useGLTF, Float, Environment, Center } from "@react-three/drei";
import * as THREE from "three";
import axios from 'axios';
import './HomePage.css';

/* =========================================================
   مكون طقم الأسنان 3D في الخلفية
   ========================================================= */
const AutoFitModel = ({ modelPath, rotation = [0, 0, 0], scale = 1, floatSpeed = 1.5 }) => {
  const { scene } = useGLTF(modelPath);
  const modelRef = useRef();

  return (
    <Center precise>
      <Float speed={floatSpeed} floatIntensity={0.5} rotationIntensity={0.2}>
        <group ref={modelRef} rotation={rotation} scale={scale}>
          <primitive object={scene} />
        </group>
      </Float>
    </Center>
  );
};

const ModelCanvas = ({ children }) => {
  return (
    <Canvas
      dpr={[1, 1.5]}
      camera={{ position: [0, 0, 10], fov: 35, near: 0.1, far: 1000 }}
      gl={{ antialias: true, alpha: true }}
      onCreated={({ gl }) => {
        gl.setClearColor(new THREE.Color(0xffffff), 0);
      }}
    >
      <ambientLight intensity={1.5} />
      <directionalLight position={[5, 8, 6]} intensity={2} />
      <directionalLight position={[-5, -3, 5]} intensity={1} />
      <Environment preset="city" environmentIntensity={0.5} />
      <Suspense fallback={null}>{children}</Suspense>
    </Canvas>
  );
};

/* =========================================================
   الصفحة الرئيسية (Dashboard)
   ========================================================= */
const HomePage = () => {
  const navigate = useNavigate();
  // حالة التحكم بفتح وإغلاق القائمة الجانبية في الموبايل
  const [isSidebarOpen, setIsSidebarOpen] = useState(false);
  const [appointments, setAppointments] = useState([]);
  const [loadingAppts, setLoadingAppts] = useState(true);
  const [apptError, setApptError] = useState("");

  useEffect(() => {
    const fetchAppointments = async () => {
      const token = localStorage.getItem("token");
      if (!token) {
        navigate("/");
        return;
      }

      try {
        const response = await axios.get("http://localhost:5113/api/Appointments", {
          headers: { Authorization: `Bearer ${token}` },
        });

        // Filter to today's appointments only
        const today = new Date();
        const todayStr = today.toISOString().split("T")[0];
        const todaysAppts = response.data.filter((appt) => {
          const apptDate = new Date(appt.startTime).toISOString().split("T")[0];
          return apptDate === todayStr;
        });

        setAppointments(todaysAppts);
      } catch (err) {
        if (err.response && err.response.status === 401) {
          localStorage.removeItem("token");
          localStorage.removeItem("role");
          navigate("/");
          return;
        }
        setApptError("Failed to load appointments.");
      } finally {
        setLoadingAppts(false);
      }
    };

    fetchAppointments();
  }, [navigate]);

  const handleLogout = () => {
    localStorage.removeItem("token");
    localStorage.removeItem("role");
    navigate('/');
  };

  const handleNewAppointment = () => {
    alert("سيتم فتح نافذة إضافة موعد جديد قريباً يا دكتور!");
  };

  const toggleSidebar = () => {
    setIsSidebarOpen(!isSidebarOpen);
  };

  return (
    <div className="dashboard-wrapper">
      
      {/* غشاوة الخلفية عند فتح القائمة في الموبايل */}
      <div 
        className={`sidebar-overlay ${isSidebarOpen ? 'open' : ''}`} 
        onClick={toggleSidebar}
      ></div>

      {/* طقم الأسنان 3D في الخلفية */}
      <div className="floating-teeth">
        <ModelCanvas>
          <AutoFitModel
            modelPath="/Teeth.glb"
            rotation={[Math.PI / 8, Math.PI / 4, 0]} 
            scale={0.8} 
          />
        </ModelCanvas>
      </div>

      {/* المحتوى الرئيسي */}
      <main className="main-content">
        
        {/* شريط علوي يظهر فقط في الموبايل */}
        <div className="mobile-header clay-panel">
          <h2 className="mobile-title">Dr. Hussein</h2>
          <button className="hamburger-btn" onClick={toggleSidebar}>
            {isSidebarOpen ? '✖' : '☰'}
          </button>
        </div>

        <header className="top-header">
          <div>
            <h1 className="greeting">Welcome back, Dr. Hussein!</h1>
            <p className="subtitle">Here is your clinic overview for today.</p>
          </div>
        </header>

        {/* بطاقات الإحصائيات */}
        <section className="stats-grid">
          <div className="stat-card clay-panel">
            <h3>Today's Patients</h3>
            <p className="stat-number">{appointments.length}</p>
          </div>
          <div className="stat-card clay-panel">
            <h3>Today's Revenue</h3>
            <p className="stat-number">$450</p>
          </div>
        </section>

        {/* قسم المواعيد اليومية */}
        <section className="appointments-section clay-panel">
          <div className="section-header">
            <h3>Today's Appointments</h3>
            <button className="clay-btn-action" onClick={handleNewAppointment}>
              + New Appointment
            </button>
          </div>
          
          <div className="table-container">
            <table className="appointments-table">
              <thead>
                <tr>
                  <th>Patient Name</th>
                  <th>Time</th>
                  <th>Treatment</th>
                  <th>Status</th>
                </tr>
              </thead>
              <tbody>
                {loadingAppts ? (
                  <tr><td colSpan="4" style={{ textAlign: "center" }}>Loading appointments...</td></tr>
                ) : apptError ? (
                  <tr><td colSpan="4" style={{ textAlign: "center", color: "#e74c3c" }}>{apptError}</td></tr>
                ) : appointments.length === 0 ? (
                  <tr><td colSpan="4" style={{ textAlign: "center" }}>No appointments for today.</td></tr>
                ) : (
                  appointments.map((appt) => (
                    <tr key={appt.id}>
                      <td>{appt.patientName}</td>
                      <td>{new Date(appt.startTime).toLocaleTimeString([], { hour: "2-digit", minute: "2-digit" })}</td>
                      <td>{appt.serviceName}</td>
                      <td>
                        <span className={`status ${appt.status.toLowerCase() === "scheduled" ? "waiting" : appt.status.toLowerCase()}`}>
                          {appt.status}
                        </span>
                      </td>
                    </tr>
                  ))
                )}
              </tbody>
            </table>
          </div>
        </section>

      </main>

      {/* الشريط الجانبي */}
      <aside className={`sidebar clay-panel ${isSidebarOpen ? 'open' : ''}`}>
        <div className="logo-container">
          <div className="profile-badge clay-panel">
            <span>Dr. Hussein</span>
          </div>
        </div>
        
        <nav className="menu">
          <button className="menu-item active clay-btn-soft" onClick={() => setIsSidebarOpen(false)}>Dashboard</button>
          <button className="menu-item clay-btn-soft" onClick={() => setIsSidebarOpen(false)}>Patients</button>
          <button className="menu-item clay-btn-soft" onClick={() => setIsSidebarOpen(false)}>Appointments</button>
          <button className="menu-item clay-btn-soft" onClick={() => setIsSidebarOpen(false)}>Billing</button>
        </nav>
        
        <div className="logout-container">
          <button className="menu-item logout-btn clay-btn-soft" onClick={handleLogout}>
            Logout
          </button>
        </div>
      </aside>

    </div>
  );
};

useGLTF.preload("/Teeth.glb");

export default HomePage;