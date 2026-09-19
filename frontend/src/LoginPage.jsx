import React, { Suspense, useRef, useState } from "react";
import { Canvas } from "@react-three/fiber";
import { useGLTF, Float, Environment, Center } from "@react-three/drei";
import * as THREE from "three";
import { useNavigate } from "react-router-dom";
import axios from "axios";
import "./LoginPage.css";

/* =========================================================
   مكون المجسمات العادية (يطفو بحرية وبسلاسة)
   ========================================================= */
const AutoFitModel = ({
  modelPath,
  rotation = [0, 0, 0],
  scale = 1,
  floatSpeed = 1.5,
  floatIntensity = 0.5,
  rotationIntensity = 0.2,
}) => {
  const { scene } = useGLTF(modelPath);
  const modelRef = useRef();

  return (
    <Center precise>
      <Float
        speed={floatSpeed}
        floatIntensity={floatIntensity}
        rotationIntensity={rotationIntensity}
      >
        <group ref={modelRef} rotation={rotation} scale={scale}>
          <primitive object={scene} />
        </group>
      </Float>
    </Center>
  );
};

/* =========================================================
   إعدادات الكاميرا والإضاءة الموحدة
   ========================================================= */
const ModelCanvas = ({ children }) => {
  return (
    <Canvas
      dpr={[1, 1.5]}
      camera={{
        position: [0, 0, 10], // بعد الكاميرا ثابت لجميع المجسمات
        fov: 35,
        near: 0.1,
        far: 1000,
      }}
      gl={{ antialias: true, alpha: true }}
      onCreated={({ gl }) => {
        gl.setClearColor(new THREE.Color(0xffffff), 0);
      }}
    >
      <ambientLight intensity={1.5} />
      <directionalLight position={[5, 8, 6]} intensity={2} />
      <directionalLight position={[-5, -3, 5]} intensity={1} />
      <Environment preset="city" environmentIntensity={0.8} />
      <Suspense fallback={null}>{children}</Suspense>
    </Canvas>
  );
};

/* =========================================================
   الصفحة الرئيسية (Login Page)
   ========================================================= */
const LoginPage = () => {
  const navigate = useNavigate();
  const [email, setEmail] = useState("");
  const [password, setPassword] = useState("");
  const [error, setError] = useState("");
  const [loading, setLoading] = useState(false);

  const handleLogin = async (e) => {
    e.preventDefault();
    setError("");
    setLoading(true);

    try {
      const response = await axios.post("http://localhost:5113/api/Auth/login", {
        email,
        password,
      });

      localStorage.setItem("token", response.data.token);
      localStorage.setItem("role", response.data.role);
      navigate('/home');
    } catch (err) {
      if (err.response) {
        // Backend returned an error response
        const data = err.response.data;
        setError(typeof data === "string" ? data : data.message || "Login failed.");
      } else {
        setError("Unable to connect to server.");
      }
    } finally {
      setLoading(false);
    }
  };

  return (
    <div className="login-wrapper">

      {/* =================================================
          الفرشاة الزرقاء (أعلى اليسار)
         ================================================= */}
      <div className="floating-model top-left">
        <ModelCanvas>
          <AutoFitModel
            modelPath="/Brush.glb"
            rotation={[1, Math.PI, -Math.PI / 4]}
            scale={0.7}
          />
        </ModelCanvas>
      </div>

      {/* =================================================
          الكرسي (أعلى اليمين)
         ================================================= */}
      <div className="floating-model top-right">
        <ModelCanvas>
          <AutoFitModel
            modelPath="/Chair.glb"
            rotation={[0, -Math.PI / 1.5, 0]}
            scale={6}
            floatSpeed={1.5}
          />
        </ModelCanvas>
      </div>

      {/* =================================================
          صندوق تسجيل الدخول (بالوسط)
         ================================================= */}
      <div className="login-box">
        <form className="login-form" onSubmit={handleLogin}>
          <input
            type="email"
            placeholder="Email"
            className="clay-input"
            required
            value={email}
            onChange={(e) => setEmail(e.target.value)}
          />
          <input
            type="password"
            placeholder="Password"
            className="clay-input"
            required
            value={password}
            onChange={(e) => setPassword(e.target.value)}
          />
          {error && <p style={{ color: "#e74c3c", margin: "0 0 10px", fontSize: "14px", textAlign: "center" }}>{error}</p>}
          <button type="submit" className="clay-btn" disabled={loading}>
            {loading ? "Logging in..." : "Login"}
          </button>
        </form>
      </div>
    </div>
  );
};

useGLTF.preload("/Chair.glb");
useGLTF.preload("/Brush.glb");
useGLTF.preload("/Majon.glb");

export default LoginPage;