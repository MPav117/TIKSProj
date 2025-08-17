import React, { useEffect, useState, useContext } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import ProfilUgovori from '../komponente/ProfilUgovori';
import ProfilOglasi from '../komponente/ProfilOglasi';
import ProfilZahteviGrupa from '../komponente/ProfilZahteviGrupa';
import ProfilZahteviPosao from '../komponente/ProfilZahteviPosao';
import { AppContext } from "../App";
import Dropdown from '../komponente/Dropdown';
import Kalendar from '../komponente/Kalendar';
import ListaProfilaMajstora from '../komponente/ListaProfilaMajstora';
import Loader from '../komponente/Loader';
import ListaRecenzija from '../komponente/ListaRecenzija';

export default function Profil() {
  //const { korisnik, setKorisnik } = useContext(AppContext);
  const [loading, setLoading] = useState(true);
  //const [error, setError] = useState(null);
  const [naProfilu, setNaProfilu] = useState(false);
  const [profileType, setProfileType] = useState(null);
  const [activeTab, setActiveTab] = useState('ugovori');
  const location = useLocation();
  const navigate = useNavigate();
  const setGlobalNaProfilu = useContext(AppContext).setNaProfilu
  const setOglas = useContext(AppContext).setOglas
  const jezik = useContext(AppContext).jezik
  const setLogovan = useContext(AppContext).setLogovan
  const [clanovi, setClanovi] = useState(null)
  const korisnik = useContext(AppContext).korisnik
  const setKorisnik = useContext(AppContext).setKorisnik
  const [recenzije, setRecenzije] = useState(null)

  useEffect(() => {
    //ovo mi treba za oglase ne brini
    setOglas(null)
    sessionStorage.removeItem('oglas')
    sessionStorage.removeItem('povezani')
    setGlobalNaProfilu(true)
    setLogovan(true)
    setLoading(false)
  }, [location.pathname, setKorisnik]);

  useEffect(() => {
    setNaProfilu(location.pathname === '/profile');
  }, [location.pathname, setNaProfilu]);

  const handleIzlazIzGrupe = async () => {
    const token = sessionStorage.getItem('jwt')
    try {
      const response = await fetch('https://localhost:7080/Majstor/izlazIzGrupe', {
        method: 'DELETE',
        headers: {
          Authorization: `bearer ${token}`
        }
      })

      if (response.ok){
        window.location.reload()
      }
      else {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  }

  if (loading) {
    return <Loader />;
  }

  // if (error) {
  //   return <p>{error}</p>;
  // }

  if (!korisnik) {
    return <p>{jezik.stranicaProfil.nema}</p>;
  }

  const loadClanovi = async () => {
    try {
      const response = await fetch(`https://localhost:7080/Majstor/GetClanovi/${korisnik.id}`)
        if (response.ok){
            const data = await response.json()
            setClanovi(data)
        }
        else {
            window.alert(jezik.general.error.netGreska + ": " + await response.text())
        }
    } catch (error) {
        window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  }

  const translateVestine = vestine => {
    if (jezik.id === 'sr'){
        return vestine.map(v => {
            switch (v) {
              case 'elektricar':
                return 'električar'
              case 'keramicar':
                return 'keramičar'
              default:
                return v
            }
          })
    }
    else {
      return vestine.map(v => {
        switch (v) {
          case 'stolar':
            return 'carpenter'
          case 'elektricar':
            return 'electrician'
          case 'vodoinstalater':
            return 'plumber'
          case 'keramicar':
            return 'tilesetter'
          default:
            return v
        }
      })
    }
  }

  const translateTip = tip => {
    if (jezik.id === 'sr'){
      return tip
    }
    else {
      switch (tip) {
        case 'majstor':
          return 'craftsman'
        case 'poslodavac':
          return 'employer'
        default:
          return tip
      }
    }
  }

  const renderTabContent = () => {
    switch (activeTab) {
      case 'ugovori':
        return <ProfilUgovori />;
      case 'oglasi':
        return <ProfilOglasi />;
      case 'zahteviZaGrupu':
        return <ProfilZahteviGrupa />
      case 'zahteviZaPosao':
        return <ProfilZahteviPosao setActiveTab={setActiveTab}/>;
      case 'kalendar':
        return <Kalendar licniProfil={true} id={korisnik.id}/>
      case 'clanovi':
        if (clanovi === null){
          loadClanovi()
        }
        return <ListaProfilaMajstora lista={clanovi} />
      case 'recenzije':
        sessionStorage.setItem('view', `${korisnik.id}`)
        return <ListaRecenzija lista={recenzije} id={korisnik.id} />
      default:
        return null;
    }
  };

  const renderStars = (rating) => {
    const stars = [];
    for (let i = 1; i <= 5; i++) {
        if (i <= rating + 0.5) {
            stars.push(<span key={i} className="text-yellow-500 text-3xl">&#9733;</span>); 
        } else {
            stars.push(<span key={i} className="text-gray-300 text-3xl">&#9733;</span>);
        }
    }
    return stars;
  };

  return (
    <div className="p-4 bg-orange-200 bg-opacity-60 -z-40 flex justify-center bg-cover bg-center h-max overflow-visible">
      <div className="h-screen"></div>
      <div className='w-full max-w-4xl flex flex-col'>
        <div className="w-full max-w-4xl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow mx-auto mt-4 flex flex-col sm:flex-row relative">
          <div className="flex flex-col items-center w-1/3 p-4 min-w-56">
            <img className="w-48 h-48 mb-3 rounded-full shadow-lg" src={korisnik.slika || "/images/"} alt="Profile" />
            <span className="text-lg text-gray-500">{translateTip(korisnik.tip)}</span>

          </div>
          <div className='absolute top-0 right-0 mt-2 mr-2 flex flex-col sm:flex-row-reverse gap-3'>
            {(korisnik.tip === 'poslodavac' || korisnik.tipMajstora === 'majstor') && (
              <div className="rounded-lg">
                <Dropdown userType={korisnik.tip} />
              </div>
            )}
            {korisnik.tip === 'majstor' && korisnik.grupa === 1 && (
              <div className='bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300'>
                <button onClick={handleIzlazIzGrupe}>{jezik.stranicaProfil.izlazIzGrupe}</button>
              </div>
            )}
          </div>
          <div className="flex flex-col justify-center w-full sm:w-2/3 p-4">
            <div className='h-0 sm:h-12'></div>
            {korisnik.tip === 'majstor' ? (
              <>
                <div className="text-left">
                  <p className="text-xl text-gray-600 font-semibold text-wrap">{korisnik.naziv}</p>
                  <p className="text-lg text-gray-600 text-wrap">{korisnik.opis}</p>
                  <p className="text-lg text-gray-600 text-wrap">{jezik.pregledi.ocena}: {renderStars(korisnik.prosecnaOcena)}</p>
                  <p className="pt-1 text-lg text-gray-600 font-semibold text-wrap">{jezik.formaProfil.vestine}: {translateVestine(korisnik.listaVestina).join(', ')}</p>
                  <p className="text-lg text-gray-600 text-wrap">Email: {korisnik.email}</p>
                  <p className="pt-5 text-lg text-gray-600 text-wrap">{jezik.stranicaProfil.iz} {korisnik.city_ascii}, {korisnik.country}</p>
                </div>
              </>
            ) : (
              <div className="text-left">
                <p className="text-xl text-gray-600 font-semibold text-wrap">{korisnik.naziv}</p>
                <p className="text-lg text-gray-600 text-wrap break-all">{korisnik.opis}</p>
                <p className="text-lg text-gray-600 text-wrap">{jezik.pregledi.ocena}: {renderStars(korisnik.prosecnaOcena)}</p>
                <p className="text-lg text-gray-600 text-wrap">Email: {korisnik.email}</p>
                <p className="pt-5 text-lg text-gray-600 text-wrap">{jezik.formaProfil.adresa}: {korisnik.adresa}, {korisnik.city_ascii}, {korisnik.country}</p>
              </div>
            )}
          </div>
        </div>
        {korisnik.tip === 'majstor' && (
          <div className="mt-4">
            <div className="flex justify-center flex-wrap gap-4 mb-4">
              <button
                className={`px-4 py-2 rounded border  ${activeTab === 'ugovori' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('ugovori')}
              >
                {jezik.stranicaProfil.ugovori}
              </button>
              <button
                className={`px-4 py-2 rounded border  ${activeTab === 'oglasi' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('oglasi')}
              >
                {jezik.stranicaProfil.oglasi}
              </button>
              <button
                className={`px-4 py-2 rounded border  ${activeTab === 'zahteviZaPosao' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('zahteviZaPosao')}
              >
                {jezik.stranicaProfil.pzahtevi}
              </button>
              {korisnik.tipMajstora === 'grupa' && (
                <button
                className={`px-4 py-2 rounded border  ${activeTab === 'clanovi' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                  onClick={() => setActiveTab('clanovi')}
                >
                  {jezik.stranicaProfil.clanovi}
                </button>  
              )}
              <button
                className={`px-4 py-2 rounded border  ${activeTab === 'zahteviZaGrupu' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('zahteviZaGrupu')}
              >
                {jezik.stranicaProfil.gzahtevi}
              </button>
              <button
                className={`px-4 py-2 rounded border  ${activeTab === 'kalendar' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('kalendar')}
              >
                {jezik.stranicaProfil.kalendar}
              </button>
              <button
                className={`px-4 py-2 rounded border  ${activeTab === 'recenzije' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('recenzije')}
              >
                {jezik.stranicaProfil.recenzije}
              </button>
            </div>
            <div className="rounded-lg">
              {renderTabContent()}
            </div>
          </div>
        )}
        {korisnik.tip === 'poslodavac' && (
          <div className="mt-4">
          <div className="flex justify-center flex-wrap gap-4 mb-4">
            <button
              className={`px-4 py-2 rounded border ${activeTab === 'ugovori' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
              onClick={() => setActiveTab('ugovori')}
            >
              {jezik.stranicaProfil.ugovori}
            </button>
            <button
              className={`px-4 py-2 rounded border ${activeTab === 'oglasi' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
              onClick={() => setActiveTab('oglasi')}
            >
              {jezik.stranicaProfil.oglasi}
            </button>
            <button
              className={`px-4 py-2 rounded border ${activeTab === 'zahteviZaPosao' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('zahteviZaPosao')}
            >
              {jezik.stranicaProfil.pzahtevi}
            </button>
            <button
                className={`px-4 py-2 rounded border  ${activeTab === 'recenzije' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                onClick={() => setActiveTab('recenzije')}
              >
                {jezik.stranicaProfil.recenzije}
              </button>
          </div>
          <div>
            {renderTabContent()}
          </div>
        </div>
      )}
      </div>
    </div>
  );
}