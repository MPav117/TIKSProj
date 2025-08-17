import React, { useState, useEffect, useContext } from 'react';
import { AppContext } from '../App';
import { useNavigate } from 'react-router-dom';
import Loader from './Loader';

export default function ProfilOglasi() {
    const [oglasi, setOglasi] = useState([]);
    const [loading, setLoading] = useState(true);
    const korisnik = useContext(AppContext).korisnik
    const setOglas = useContext(AppContext).setOglas
    const jezik = useContext(AppContext).jezik
    const navigate = useNavigate()
    const setNaProfilu = useContext(AppContext).setNaProfilu
    const [isDeleteConfirmOpen, setIsDeleteConfirmOpen] = useState(false)
    const [oglasZaBrisanje, setOglasZaBrisanje] = useState(null)
    const [odjavaPrompt, setOdjavaPrompt] = useState(false)
    const [oglasOdjava, setOglasOdjava] = useState(null)
    
    const fetchOglasi = async () => {
        try {
            const response = await fetch('https://localhost:7080/Korisnik/getOglasi', {
                headers: {
                    'Authorization': `bearer ${sessionStorage.getItem('jwt')}` 
                }
            });

            if (!response.ok) {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }

            const data = await response.json();

            if (Array.isArray(data)) { //ako je niz
                setOglasi(data);
                setLoading(false);
            } else {
                window.alert(jezik.general.error.netGreska + ": Unexpected response format")
                setLoading(false);
            }
        } catch (err) {
            window.alert(jezik.general.error.netGreska + ": " + err.message)
            setLoading(false);
        }
    };

    useEffect(() => {
        fetchOglasi();
    }, []);

    const selectOglas = ogl => {
      setNaProfilu(false)
      setOglas(ogl)
      sessionStorage.setItem('oglas', `${ogl.id}`)
      navigate('../view_job_posting')
    }

    const handleBrisanjeOglasa = (id) => {
      setIsDeleteConfirmOpen(true)
      setOglasZaBrisanje(id)
    }

    const handleBrisanjeOglasaConfirm = async id => {
      const token = sessionStorage.getItem('jwt')
      try {
          const response = await fetch(`https://localhost:7080/Poslodavac/IzbrisatiOglas/${id}`, {
              method: 'DELETE',
              headers: {
                  Authorization: `bearer ${token}`
                }
          })
          if (response.ok){
            fetchOglasi()
          }
          else {
              window.alert(jezik.general.error.netGreska + ": " + await response.text())
          }
      } catch (error) {
          window.alert(jezik.general.error.netGreska + ": " + error.message)
      }
      setIsDeleteConfirmOpen(false)
    }

    const handleBrisanjeOglasaCancel = () => {
      setIsDeleteConfirmOpen(false)
    }

    const handleOdjava = id => {
      setOdjavaPrompt(true)
      setOglasOdjava(id)
    }

    const handleOdjavaConfirm = async id => {
      const token = sessionStorage.getItem('jwt')
      try {
          const response = await fetch(`https://localhost:7080/Majstor/OdjaviSaOglasa/${id}`, {
              method: 'DELETE',
              headers: {
                  Authorization: `bearer ${token}`
                }
          })
          if (response.ok){
              fetchOglasi()
          }
          else {
              window.alert(jezik.general.error.netGreska + ": " + await response.text())
          }
      } catch (error) {
          window.alert(jezik.general.error.netGreska + ": " + error.message)
      }
      setOdjavaPrompt(false)
    }

    const handleOdjavaCancel = () => {
      setOdjavaPrompt(false)
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

    if (loading) {
        return <Loader />
    }
    return (
      <div className="w-full max-w-4xl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow mx-auto mt-4 container">
      {oglasi.length === 0 ? (
        <h2 className='text-center pt-5 text-2xl font-semibold'>{jezik.profilOglasi.nema}</h2>
      ) : (
        <div className="gap-4 px-5 pt-5 flex flex-col justify-center">
          <h2 className='text-center text-2xl font-semibold'>{jezik.profilOglasi.oglasi}</h2>
          {oglasi.map(oglas => (
            <div key={oglas.id} className="bg-orange-100 border border-orange-200 shadow-sm p-4 mb-2 rounded cursor-pointer w-full text-center">
              <h3 className="text-xl font-bold mb-2">{oglas.naslov}</h3>
              <p className="text-gray-700 mb-2 text-wrap">{oglas.opis}</p>
              <p className="text-gray-700 mb-2 text-wrap">{jezik.profilOglasi.vestine}: {translateVestine(oglas.listaVestina).join(', ')}</p>
              <p>
                <span className="text-gray-700 mb-2 pe-3">{jezik.profilOglasi.pdate}: {new Date(oglas.datumPostavljanja).toLocaleDateString()}</span>
                <span className="text-gray-700 mb-2 ps-3">{jezik.profilUgovori.dkraj}: {new Date(oglas.datumZavrsetka).toLocaleDateString()}</span>
              </p>
              <p className="text-gray-700 mb-2">{jezik.pregledi.plata}: {oglas.cenaPoSatu} EUR</p>
              {oglas.listaSlika && oglas.listaSlika.length > 0 && (
                <div className="flex justify-center">
                  <div className="flex overflow-x-auto">
                    {oglas.listaSlika.map((slika, index) => (
                      <img
                        key={index}
                        src={slika}
                        alt={`Slika ${index + 1}`}
                        className="h-32 w-auto object-cover mr-2 mb-2 rounded"
                      />
                    ))}
                  </div>
                </div>
              )}
              <div className='flex flex-wrap justify-center gap-5'>
                <button onClick={() => selectOglas(oglas)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300">
                  {jezik.profilOglasi.pogledaj}
                </button>
                {korisnik !== null && korisnik.tip !== 'poslodavac' && oglas.prijavljeni !== null && oglas.prijavljeni.includes(korisnik.id) && !oglas.pripadaGrupi && (
                  <button onClick={() => handleOdjava(oglas.id)} className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">
                    {jezik.profilOglasi.odjavi}
                  </button>
                )}
                {korisnik !== null && korisnik.tip === 'poslodavac' && (
                  <button onClick={() => handleBrisanjeOglasa(oglas.id)} className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">
                    {jezik.profilOglasi.izbrisi}
                  </button>
                )}

              </div>
            </div>
          ))}
        </div>
      )}
        {isDeleteConfirmOpen && (
          <div className="fixed inset-0 flex items-center justify-center">
            <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
            <div className="bg-white p-4 rounded-lg shadow-lg w-96 relative border border-yellow-600">
              <p className="mb-4"> {/*{jezik.profilHeader.confirmDeleteOglas} */}{jezik.profilOglasi.prompt}</p>
              <div className="flex justify-end space-x-4">
                <button onClick={() => handleBrisanjeOglasaConfirm(oglasZaBrisanje)} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
                <button onClick={handleBrisanjeOglasaCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
              </div>
            </div>
          </div>
        )}
        {odjavaPrompt && (
            <div className="fixed inset-0 flex items-center justify-center">
              <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
              <div className="bg-white p-4 rounded-lg shadow-lg w-96 relative border border-yellow-600">
                <p className="mb-4">{jezik.profilOglasi.propmtodjava}</p>
                <div className="flex justify-end space-x-4">
                  <button onClick={() => handleOdjavaConfirm(oglasOdjava)} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
                  <button onClick={handleOdjavaCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
                </div>
              </div>
            </div>
          )}
        </div>
      );
}