import React, { useContext, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AppContext } from '../App';

export default function Dropdown ({ userType }) {
  const navigate = useNavigate()
  const korisnik = useContext(AppContext).korisnik
  const setNaProfilu = useContext(AppContext).setNaProfilu
  const jezik = useContext(AppContext).jezik

  const handleDropdownItemClick = (tip, povezan) => {
    setNaProfilu(false)
    if (!povezan){
      sessionStorage.setItem('povezani', `${korisnik.id}`)
      navigate(tip === 'm' ? '../../register_employer' : '../../register_craftsman')
    }
    else {
      navigate('../login')
    }
  };

  return (
    <div className="bg-white rounded-lg shadow-lg z-30">
      {korisnik.povezani === 0 ? 
        userType === 'majstor' ? (
          <button onClick={() => handleDropdownItemClick('m', false)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300">{jezik.dropdown.rasp}</button>
        ) : (
          <button onClick={() => handleDropdownItemClick('p', false)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300">{jezik.dropdown.rasm}</button>
        ) : 
        userType === 'majstor' ? (
          <button onClick={() => handleDropdownItemClick('m', true)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300">{jezik.dropdown.switch}</button>
        ) : (
          <button onClick={() => handleDropdownItemClick('p', true)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300">{jezik.dropdown.switch}</button>
        )
      }
    </div>
  );
};

