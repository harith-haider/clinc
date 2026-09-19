import React from 'react';
import { BrowserRouter as Router, Routes, Route } from 'react-router-dom';
import LoginPage from './LoginPage';
import HomePage from './HomePage'; // تأكد أن ملف HomePage.jsx موجود بنفس المجلد

function App() {
  return (
    <Router>
      <Routes>
        {/* المسار الافتراضي (/) يعرض صفحة تسجيل الدخول */}
        <Route path="/" element={<LoginPage />} />
        
        {/* مسار (/home) يعرض الصفحة الرئيسية (الداشبورد) */}
        <Route path="/home" element={<HomePage />} />
      </Routes>
    </Router>
  );
}

export default App;