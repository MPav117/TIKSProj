import React, { useContext } from 'react';
import { Link } from 'react-router-dom';
import { AppContext } from '../App';

export default function Pocetna(props){
  const jezik = useContext(AppContext).jezik
  return (
    <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible relative">
      <div className="h-screen"></div>
      <div className="absolute inset-0 bg-gradient-to-r from-transparent via-gray-100 to-transparent opacity-80 z-10">  {/* Prozirni sloj s z-index */}
        <div className="flex flex-col justify-center items-center h-full">
          <h1 className="text-9xl text-amber-800 font-bold">ufindi</h1>
          <span className="mt-4 text-amber-700 text-2xl pb-5">{jezik.pocetna.moto}</span>
          <div className='flex flex-col gap-8 sm:flex-row'>
            <Link to='/search_craftsmen' className='backdrop-blur-sm font-semibold bg-opacity-96  block h-20 p-5 text-xl bg-yellow-800 border-2 border-yellow-800 rounded-lg shadow-lg text-white hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'>
              <span className='align-middle'>{jezik.pocetna.pregledMajstora}</span>
            </Link>
            <Link to='/search_job_postings' className='backdrop-blur-sm font-semibold bg-opacity-96  block h-20 p-5 text-xl bg-yellow-800 border-2 border-yellow-800 rounded-lg shadow-lg text-white hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'>
              <span className='align-middle'>{jezik.pocetna.pregledOglasa}</span>
            </Link>
          </div>
        </div>
      </div>
      <footer className="bg-gradient-to-r from-amber-800 to-amber-600 text-white text-center py-4 absolute -bottom-0 w-full z-20"> 
        © DAMS 2024 
      </footer>
    </div>
  );
};
