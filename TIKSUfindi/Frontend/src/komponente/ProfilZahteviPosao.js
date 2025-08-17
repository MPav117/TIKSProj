
import React, { useContext, useEffect, useState } from 'react';
import { useNavigate } from 'react-router-dom';
import { AppContext } from '../App';
import Loader from './Loader';

export default function ProfilZahteviPosao(props) {
  const [zahtevi, setZahtevi] = useState([]);
  const [loading, setLoading] = useState(true);
  //const [error, setError] = useState(null);
  const navigate = useNavigate();
  const korisnik = useContext(AppContext).korisnik
  const jezik = useContext(AppContext).jezik
  const setNaProfilu = useContext(AppContext).setNaProfilu
  const [isPovuciZahtevConfirmOpen, setPovuciZahtevConfirmOpen] = useState(false)
  const [zahtevzapov, setZahtevzapov] = useState(null)


  const fetchZahtevi = async () => {
    const token = sessionStorage.getItem("jwt");
    try {
      const response = await fetch('https://localhost:7080/Korisnik/getZahteviPosaoMajstorGrupa', {
        headers: {
          Authorization: `bearer ${token}`
        }
      });

      if (!response.ok) {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }

      const data = await response.json();
      setZahtevi(data)   //setZahtevi(data["$values"]); meni nema ovo $values samo vrati niz normalno i sa ovim puca jer je undefined
      
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    } finally {
      setLoading(false)
    }
  }

  useEffect(() => {
    fetchZahtevi();
  }, []);

  const handleResponse = async (zahtevId, odgovor) => {
    const token = sessionStorage.getItem("jwt");
    try {
      const response = await fetch(`https://localhost:7080/Majstor/OdgovorZahtevPosao/${zahtevId}/${odgovor}`, {
        method: "GET",
        headers: {
          Authorization: `bearer ${token}`
        }
      });

      if (!response.ok) {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
      else {
        //fetchZahtevi()
        props.setActiveTab('ugovori')
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  };

  const handlePovuci = (id) => {
    setPovuciZahtevConfirmOpen(true);
    setZahtevzapov(id)
  };

  const handlePovuciConfirm = async id => {
    const token = sessionStorage.getItem('jwt')
    try {
      const response = await fetch(`https://localhost:7080/Poslodavac/povuciZahtevPosao/${id}`, {
        method: 'DELETE',
        headers: {
          Authorization: `bearer ${token}`
        }
      })

      if (response.ok){
        fetchZahtevi()
      }
      else {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
    setPovuciZahtevConfirmOpen(false)
  }

  const handlePovuciCancel = () => {
    setPovuciZahtevConfirmOpen(false);
  };
  
  if (loading) {
    return <Loader />;
  }

  const handleChatClick = (drugaStranaID) => {
    let chatRoom;
    if(korisnik.tip== "majstor"){
     chatRoom = `${drugaStranaID}_${korisnik.id}`;
    }
    else if(korisnik.tip=="poslodavac"){
       chatRoom=  `${korisnik.id}_${drugaStranaID}`
    }
    navigate('/chat', { state: { chatRoom } });
  };

  const handleViewKorisnik = id => {
    sessionStorage.setItem('view', `${id}`)
    setNaProfilu(false)
    navigate(`../view_profile`)
  }

  return (
    <div className="w-full max-w-4xl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow mx-auto mt-4 container">
      {zahtevi.length === 0 ? (
        <h2 className='text-center py-5 text-2xl font-semibold'>{jezik.profilZahtevi.nemap}</h2>
      ) : (
        <div className="gap-4 p-5 flex flex-col justify-center">
          <h2 className='text-center text-2xl font-semibold'>{jezik.profilZahtevi.zahtevip}</h2>
          {zahtevi.map((zahtev) => (
            <div
              key={zahtev.id}
              className="bg-orange-100 border border-orange-200 shadow-sm p-4 mb-2 rounded cursor-pointer w-full text-center"
              style={{ color: 'black' }}
            >
              {korisnik.tip === 'majstor' && (
                <p onClick={() => handleViewKorisnik(zahtev.drugaStranaID)} className="text-xl font-bold mb-2">{jezik.profilZahtevi.posiljalac}: {zahtev.drugaStranaNaziv || 'N/A'}</p>
              )}
              {korisnik.tip === 'poslodavac' && (
                <p onClick={() => handleViewKorisnik(zahtev.drugaStranaID)} className="text-xl font-bold mb-2">{jezik.profilZahtevi.primalac}: {zahtev.drugaStranaNaziv || 'N/A'}</p>
              )}
              <p className="text-gray-700 mb-2 text-wrap">{zahtev.opis}</p>
              <p className="text-gray-700 mb-2">{jezik.pregledi.plata}: {zahtev.cenaPoSatu} EUR</p>
              <p className="text-gray-700 mb-2">{jezik.profilUgovori.dkraj}: {new Date(zahtev.datumZavrsetka).toLocaleDateString()}</p>
              {zahtev.listaSlika && zahtev.listaSlika.length > 0 && (
                <div className="flex justify-center">
                  <div className="flex overflow-x-auto">
                    {zahtev.listaSlika.map((slika, index) => (
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
                {!zahtev.zahtevGrupe && (
                  <button onClick={() => handleChatClick(zahtev.drugaStranaID, korisnik.id)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:yellow-orange-700 hover:text-white transition-colors duration-300">
                    {jezik.profilZahtevi.chat} 
                  </button>
                )}
                {korisnik.tip === 'majstor' && zahtev.prihvacen === 0 && !zahtev.zahtevGrupe && (
                  <div className="flex space-x-2">
                    <button
                      onClick={() => handleResponse(zahtev.id, 'da')}
                      className="bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-4 py-4 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300"
                    >
                      {jezik.profilZahtevi.prihvati}
                    </button>
                    <button
                      onClick={() => handleResponse(zahtev.id, 'ne')}
                      className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300"
                    >
                      {jezik.profilZahtevi.odbiji}
                    </button>
                  </div>
                )}
                {korisnik.tip === 'poslodavac' && zahtev.prihvacen === 0 && (
                  <div>
                    <button
                      onClick={() => handlePovuci(zahtev.id)}
                      className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300"
                    >
                      {jezik.profilZahtevi.povuci}
                    </button>
                  </div>
                )}
              </div>
            </div>
          ))}
        </div>
      )}
      {isPovuciZahtevConfirmOpen && (
        <div className="fixed inset-0 flex items-center justify-center">
          <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
          <div className="bg-white p-4 rounded-lg shadow-lg w-80 relative border border-yellow-600">
            <p className="mb-4 text-center">{jezik.profilZahtevi.promptp}</p>
            <div className="flex justify-end space-x-4">
              <button onClick={() => handlePovuciConfirm(zahtevzapov)} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
              <button onClick={handlePovuciCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
            </div>
          </div>
        </div>
      )}   
    </div>
  
  );
}
