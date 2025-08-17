import React, { useState, useEffect, useContext } from 'react';
import { AppContext } from '../App';
import { useNavigate } from 'react-router-dom';
import Loader from './Loader';

const ProfilZahteviGrupa = () => {
  const [zahtevi, setZahtevi] = useState({ poslati: [], primljeni: [] });
  const [loading, setLoading] = useState(true);
  const korisnik = useContext(AppContext).korisnik
  const setNaProfilu = useContext(AppContext).setNaProfilu
  const navigate = useNavigate()
  const jezik = useContext(AppContext).jezik
  const [isPovuciConfirmOpen, setIsPovuciConfirmOpen] = useState(false)
  const [zahtevzapovlacenje, setZahtevzapovlacenje] = useState(null)

  const fetchZahteviGrupa = async () => {
    const token = sessionStorage.getItem("jwt");
    try {
      const response = await fetch('https://localhost:7080/Majstor/GetZahteviGrupa', {
        method: 'GET',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `Bearer ${token}`,
        },
      });

      if (!response.ok) {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }

      const data = await response.json();
      setZahtevi(data);
      setLoading(false);
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  };

  useEffect(() => {
    fetchZahteviGrupa();
  }, [zahtevi]);

  const handleResponse = async (id, odg) => {
    const token = sessionStorage.getItem("jwt");
    try {
      const response = await fetch(`https://localhost:7080/Majstor/odgovorZahtevGrupa/${id}/${odg}`, {
        method: 'PUT',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
      
      if (response.ok){
        fetchZahteviGrupa()
      }
      else {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  }

  const handlePovuci = (id) => {
    setIsPovuciConfirmOpen(true);
    setZahtevzapovlacenje(id)
  };

  const handlePovuciConfirm = async id => {
    const token = sessionStorage.getItem("jwt");
    try {
      const response = await fetch(`https://localhost:7080/Majstor/povuciZahtevGrupa/${id}`, {
        method: 'DELETE',
        headers: {
          Authorization: `Bearer ${token}`,
        },
      });
  
      if (response.ok){
        fetchZahteviGrupa()
      }
      else {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
    setIsPovuciConfirmOpen(false)
  }

  const handlePovuciCancel = () => {
    setIsPovuciConfirmOpen(false);
  };

  const handleNapraviGrupu = async id => {
    setNaProfilu(false)
    navigate('../register_craftsman_group')
  }

  const handleViewKorisnik = id => {
    sessionStorage.setItem('view', `${id}`)
    setNaProfilu(false)
    navigate(`../view_profile`)
  }

  if (loading) {
    return <Loader />
  }

  return (
    <div className="w-full max-w-4xl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow mx-auto mt-4 container">
      {zahtevi.poslatiZahtevi.length === 0 && zahtevi.primljeniZahtevi.length === 0 ? (
        <h2 className='text-center py-5 text-2xl font-semibold'>{jezik.profilZahtevi.nemag}</h2>
      ) : (
        <>
          {zahtevi.poslatiZahtevi.length === 0 ? (
            <h2 className='text-center pt-5 text-2xl font-semibold'>{jezik.profilZahtevi.nemagpos}</h2>
          ) : (
            <div className="gap-4 p-5 flex flex-col justify-center">
              <h2 className='text-center text-2xl font-semibold'>{jezik.profilZahtevi.poslatig}</h2>
                {zahtevi.poslatiZahtevi.map((zahtev, index) => (
                  <div
                    key={index}
                    className="bg-orange-100 border border-orange-200 shadow-sm p-4 mb-2 rounded cursor-pointer w-full text-center"
                  >
                    <p onClick={() => handleViewKorisnik(zahtev.korisnik)} className="text-xl font-bold mb-2">{jezik.profilZahtevi.primalac}: {zahtev.naziv || 'N/A'}</p>
                    <p className="text-gray-700 mb-2 text-wrap">{zahtev.opis}</p>
                    {/* <span className="font-semibold">Tip:</span> {zahtev.tip}, <span className="font-semibold">Korisnik:</span> {zahtev.korisnik}, <span className="font-semibold">Prihvacen:</span> {zahtev.prihvacen ? 'Da' : 'Ne'} */}
                    <div className='flex flex-wrap justify-center gap-5'>
                      {zahtev.prihvacen === 0 ? (
                        <button
                          onClick={() => handlePovuci(zahtev.id)}
                          className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300"
                        >
                          {jezik.profilZahtevi.povuci}
                        </button>
                      ) : korisnik.tipMajstora === 'majstor' ? (
                        <button
                          onClick={() => handleNapraviGrupu(zahtev.id)}
                          className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300"
                        >
                          {jezik.profilZahtevi.napravig}
                        </button>
                      ) : (
                        <></>
                      )}
                    </div>
                  </div>
                ))}
            </div>
          )}
    
          {isPovuciConfirmOpen && (
            <div className="fixed inset-0 flex items-center justify-center">
              <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
              <div className="bg-white p-4 rounded-lg shadow-lg w-96 relative border border-yellow-600">
                <p className="mb-4">{jezik.profilZahtevi.promptg}</p>
                <div className="flex justify-end space-x-4">
                  <button onClick={() => handlePovuciConfirm(zahtevzapovlacenje)} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
                  <button onClick={handlePovuciCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
                </div>
              </div>
            </div>
          )}
    
          {zahtevi.primljeniZahtevi.length === 0 ? (
            <h2 className='text-center py-5 text-2xl font-semibold'>{jezik.profilZahtevi.nemagpri}</h2>
          ) : (
            <div className="gap-4 p-5 flex flex-col justify-center">
              <h2 className='text-center text-2xl font-semibold'>{jezik.profilZahtevi.primljenig}</h2>
                {zahtevi.primljeniZahtevi.map((zahtev, index) => (
                  <div
                    key={index}
                    className="bg-orange-100 border border-orange-200 shadow-sm p-4 mb-2 rounded cursor-pointer w-full text-center"
                  >
                    <p onClick={() => handleViewKorisnik(zahtev.korisnik)} className="text-xl font-bold mb-2">{jezik.profilZahtevi.posiljalac}: {zahtev.naziv || 'N/A'}</p>
                    <p className="text-gray-700 mb-2 text-wrap">{zahtev.opis}</p>
                    <div className='flex flex-wrap justify-center gap-5'>
                      {zahtev.prihvacen === 0 && (
                        <div className="flex space-x-2">
                          <button
                            onClick={() => handleResponse(zahtev.id, 1)}
                            className="bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-4 py-4 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300"
                          >
                            {jezik.profilZahtevi.prihvati}
                          </button>
                          <button
                            onClick={() => handleResponse(zahtev.id, 0)}
                            className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300"
                          >
                            {jezik.profilZahtevi.odbiji}
                          </button>
                        </div>
                      )}
                    </div>
                  </div>
                ))}
            </div>
          )}
        </>
      )}
    </div>
  );
};

export default ProfilZahteviGrupa