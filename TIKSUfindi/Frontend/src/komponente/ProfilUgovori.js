import React, { useContext, useEffect, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { AppContext } from '../App';
import jsPDF from 'jspdf'; //a library to generate PDFs in client-side JavaScript.
import Loader from './Loader';

export default function ProfilUgovori() {
  const [ugovori, setUgovori] = useState([]);
  const [loading, setLoading] = useState(true);
  const [isTerminateConfirmOpen, setIsTerminateConfirmOpen] = useState(false)
  const location = useLocation();
  const navigate = useNavigate();
  const korisnik = useContext(AppContext).korisnik
  const setNaProfilu = useContext(AppContext).setNaProfilu
  const jezik = useContext(AppContext).jezik
  const [idZaRaskid, setIdZaRaskid] = useState(null)
  
  const fetchUgovori = async () => {
    const token = sessionStorage.getItem("jwt");
    try {
      const response = await fetch('https://localhost:7080/Korisnik/getUgovori', {
        headers: {
          Authorization: `bearer ${token}`
        }
      });

      if (!response.ok) {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
      else {
        const data = await response.json();
        setUgovori(data)
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    } finally {
      setLoading(false);
    }
  };

  useEffect(() => {
    fetchUgovori();
  }, []);

  const handleUgovorClick = (ugovor) => {
    setNaProfilu(false)
    navigate(`../ugovor`, { state: { ugovor } });
  };

  const downloadPDF = async (ugovor) => {
    const doc = new jsPDF();
    const options = {
      align: 'left',
      fontSize: 1,
      fontStyle: 'italic'
    };
    const content =`${jezik.ugovor.a}${ugovor.imePoslodavca}${jezik.ugovor.b}${ugovor.imeMajstora}${jezik.ugovor.c}${ugovor.opis}${jezik.ugovor.d}${new Date(ugovor.datumPocetka).toLocaleDateString()}${jezik.ugovor.e}${new Date(ugovor.datumZavrsetka).toLocaleDateString()}${jezik.ugovor.f}${ugovor.cenaPoSatu}${jezik.ugovor.g}${jezik.ugovor.h}${jezik.ugovor.i}${jezik.ugovor.j}`;
    const splitText = doc.splitTextToSize(content, 250);
    doc.setFontSize(12)
    doc.setFont("times")
    //doc.text(splitText, 10, 10, {align: 'center'});
    var xOffset = doc.internal.pageSize.width/2 
            doc.text(splitText, xOffset,10, {align: 'center'})
  
    if (ugovor.potpisMajstor) {
      const potpisMajstoraImg = await loadImage(ugovor.potpisMajstor);
      doc.addImage(potpisMajstoraImg, 'JPEG', 10, 250, 50, 20);
      doc.text(`${jezik.ugovor.potpism}`, 10, 245);
    }
  
    if (ugovor.potpisPoslodavca) {
      const potpisPoslodavcaImg = await loadImage(ugovor.potpisPoslodavca);
      doc.addImage(potpisPoslodavcaImg, 'JPEG', doc.internal.pageSize.width - 60, 250, 50, 20);
      doc.text(`${jezik.ugovor.potpisp}`, doc.internal.pageSize.width - 60, 245);
    }
  // Dodavanje watermark-a na različite pozicije na stranici
  const watermark = 'ufindi';
  const watermarkFontSize = 12; // Veličina fonta za watermark
  const watermarkColor = 200; // Boja fonta za watermark

  doc.setFontSize(watermarkFontSize);
  doc.setTextColor(watermarkColor);

  const pageWidth = doc.internal.pageSize.width;
  const pageHeight = doc.internal.pageSize.height;
  const watermarkXIncrement = 50; // Inkrement za X poziciju
  const watermarkYIncrement = 50; // Inkrement za Y poziciju

  let xPosition = 10; // Inicijalna X pozicija
  let yPosition = 10; // Inicijalna Y pozicija

  while (yPosition < pageHeight) {
    doc.text(watermark, xPosition, yPosition);
    xPosition += watermarkXIncrement;

    if (xPosition > pageWidth) {
      xPosition = 10;
      yPosition += watermarkYIncrement;
    }
  }
  
  
    doc.save(`ugovor_${ugovor.id}.pdf`);
  };

  const loadImage = (base64) => {
    return new Promise((resolve) => {
      const img = new Image();
      img.src = base64;
      img.onload = () => resolve(img);
    });
  };

  const handleAddRecenzija = (uID, kID1, kID2) => {
    sessionStorage.setItem('uID', `${uID}`)
    if (korisnik.tip === 'majstor'){
      sessionStorage.setItem('kID', `${kID2}`)
    }
    else {
      sessionStorage.setItem('kID', `${kID1}`)
    }
    setNaProfilu(false)
    navigate('/add_review')
  }

  const handleRaskini = (id) => {
    setIsTerminateConfirmOpen(true)
    setIdZaRaskid(id)
  };

  const handleRaskiniConfirm = async id => {
    const token = sessionStorage.getItem('jwt')
    try {
      const response = await fetch(`https://localhost:7080/Korisnik/raskiniUgovor?idUgovora=${id}`, {
        method: 'PUT',
        headers: {
          Authorization: `bearer ${token}`
        }
      })

      if (response.ok){
        fetchUgovori()
      }
      else {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
    setIsTerminateConfirmOpen(false)
  }

  const handleRaskiniCancel = () => {
    setIsTerminateConfirmOpen(false)
  };

  const handleZavrsenPosao = async (uspeh, id) => {
    const token = sessionStorage.getItem('jwt')
    try {
      const response = await fetch(`https://localhost:7080/Poslodavac/zavrsiPosao/${id}/${uspeh}`, {
        method: 'POST',
        headers: {
          Authorization: `bearer ${token}`
        }
      })

      if (response.ok){
        fetchUgovori()
      }
      else {
        window.alert(jezik.general.error.netGreska + ": " + await response.text())
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  }

  const handleViewKorisnik = (idm, idp) => {
    if (korisnik.tip === 'majstor'){
      sessionStorage.setItem('view', `${idp}`)
    }
    else {
      sessionStorage.setItem('view', `${idm}`)
    }
    setNaProfilu(false)
    navigate(`../view_profile`)
  }

  const translateStatus = status => {
    switch (jezik.id) {
      case 'en':
        switch (status) {
            case 'nepotpisan':
              return 'Unsigned'
            case 'potpisaoMajstor':
              return 'Signed by Craftsman'
            case 'potpisaoPoslodavac':
              return 'Signed by Employer'
            case 'potpisan':
              return 'Signed'
            case 'raskidaMajstor':
              return 'Terminated by Craftsman'
            case 'raskidaPoslodavac':
              return 'Terminated by Employer'
            case 'neuspesnoZavrsen':
              return 'Unsuccessfuly Finished'
            case 'uspesnoZavrsen':
              return 'Successfuly Finished'
            default:
              return status
        }
      case 'sr':
        switch (status) {
          case 'nepotpisan':
            return 'Nepotpisan'
          case 'potpisaoMajstor':
            return 'Potpisao Majstor'
          case 'potpisaoPoslodavac':
            return 'Potpisao poslodavac'
          case 'potpisan':
            return 'Potpisan'
          case 'raskidaMajstor':
            return 'Raskida Majstor'
          case 'raskidaPoslodavac':
            return 'Raskida Poslodavac'
          case 'neuspesnoZavrsen':
            return 'Neuspesno Zavrsen'
          case 'uspesnoZavrsen':
            return 'Uspesno Zavrsen'
          default:
            return status
      }
      default:
        return status
    }
  }

  if (loading) {
    return <Loader />
  }

  return (
<div className="w-full max-w-4xl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow mx-auto mt-4 container">
      {ugovori.length === 0 ? (
        <h2 className='text-center py-5 text-2xl font-semibold'>{jezik.profilUgovori.nema}</h2>
      ) : (
        <div>
          <div className="gap-4 px-5 pt-5 flex flex-col justify-center">
            <h2 className='text-center text-2xl font-semibold'>{jezik.profilUgovori.aktivni}</h2>
            {ugovori.map(ugovor => {
              if (ugovor.status !== 'neuspesnoZavrsen' && ugovor.status !== 'uspesnoZavrsen'){
                return (
                  <div key={ugovor.id} className="bg-orange-100 border border-orange-200 shadow-sm p-4 mb-2 rounded w-full text-center">
                    <h3 onClick={() => handleViewKorisnik(ugovor.majstorID, ugovor.poslodavacID)} className="text-xl font-bold mb-2">{ugovor.status === 'potpisan' ? `${ugovor.imePoslodavca} - ${ugovor.imeMajstora}` : `${korisnik.naziv} - ${ugovor.imeZaPrikaz}`}</h3>
                    {(ugovor.status === 'potpisan' || ugovor.status === 'raskidaMajstor' || ugovor.status=== 'raskidaPoslodavac' || (ugovor.status === 'potpisaoMajstor' && korisnik.tip === 'majstor') || (korisnik.tip === 'poslodavac' && ugovor.status === 'potpisaoPoslodavac')) && (
                      <>
                        <p className="text-gray-700 mb-2 text-wrap">{ugovor.opis}</p>
                        <p>
                          <span className="text-gray-700 mb-2 pe-3">{jezik.profilUgovori.dpocetak}: {new Date(ugovor.datumPocetka).toLocaleDateString()}</span>
                          <span className="text-gray-700 mb-2 ps-3">{jezik.profilUgovori.dkraj}: {new Date(ugovor.datumZavrsetka).toLocaleDateString()}</span>
                        </p>
                        <p className="text-gray-700 mb-2">{jezik.pregledi.plata}: {ugovor.cenaPoSatu} EUR</p>
                      </>
                    )}
                    <p className="text-gray-700 mb-2 font-semibold">Status: {translateStatus(ugovor.status)}</p>
                    <div className='flex flex-wrap justify-center gap-5'>
                      {ugovor.status !== 'nepotpisan' && ugovor.status !== 'potpisaoMajstor' && ugovor.status !== 'potpisaoPoslodavac' && (
                        <button onClick={() => downloadPDF(ugovor)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.download}</button>
                      )}
                      {(ugovor.status === 'nepotpisan' || (ugovor.status === 'potpisaoMajstor' && korisnik.tip === 'poslodavac') || (korisnik.tip === 'majstor' && ugovor.status === 'potpisaoPoslodavac')) && !ugovor.pripadaGrupi && (
                        <button onClick={() => handleUgovorClick(ugovor)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.potpisi}</button>
                      )}
                      {(ugovor.status === 'potpisan' || ugovor.status === 'raskidaMajstor' || ugovor.status=== 'raskidaPoslodavac') && !ugovor.pripadaGrupi && (
                        <button onClick={() => handleAddRecenzija(ugovor.id, ugovor.majstorID, ugovor.poslodavacID)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.recenzija}</button>
                      )}
                      {(ugovor.status === 'potpisan' || (ugovor.status === 'raskidaMajstor' && korisnik.tip === 'poslodavac') || (korisnik.tip === 'majstor' && ugovor.status === 'raskidaPoslodavac')) && !ugovor.pripadaGrupi && (
                        <button onClick={() => handleRaskini(ugovor.id)} className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.raskini}</button>
                      )}
                      {(ugovor.status === 'potpisan' && korisnik.tip === 'poslodavac') && (
                        <>
                          <button onClick={() => handleZavrsenPosao(0, ugovor.id)} className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-4 py-4 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.neuspeh}</button>
                          <button onClick={() => handleZavrsenPosao(1, ugovor.id)} className="bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-4 py-4 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.uspeh}</button>
                        </>
                      )}

                    </div>
                  </div>
                )
                }
                })}
          </div>
          {isTerminateConfirmOpen && (
            <div className="fixed inset-0 flex items-center justify-center">
              <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
              <div className="bg-white p-4 rounded-lg shadow-lg w-96 relative border border-yellow-600">
                <p className="mb-4"> {/*{jezik.profilHeader.confirmTerminate}*/} {jezik.profilUgovori.prompt}</p>
                <div className="flex justify-end space-x-4">
                  <button onClick={() => handleRaskiniConfirm(idZaRaskid)} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
                  <button onClick={handleRaskiniCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
                </div>
              </div>
            </div>
          )}
          <div className="gap-4 px-5 py-5 flex flex-col justify-center">
            <h2 className='text-center text-2xl font-semibold'>{jezik.profilUgovori.zavrseni}</h2>
            {ugovori.map(ugovor => {
              if (ugovor.status === 'neuspesnoZavrsen' || ugovor.status === 'uspesnoZavrsen'){
                return (
                  <div key={ugovor.id} className="bg-orange-100 border border-orange-200 shadow-sm p-4 mb-2 rounded w-full text-center">
                    <h3 onClick={() => handleViewKorisnik(ugovor.majstorID, ugovor.poslodavacID)} className="text-xl font-bold">{ugovor.imePoslodavca} - {ugovor.imeMajstora}</h3>
                    <p className="text-gray-700 mb-2 text-wrap">{ugovor.opis}</p>
                        <p>
                          <span className="text-gray-700 mb-2 pe-3">{jezik.profilUgovori.dpocetak}: {new Date(ugovor.datumPocetka).toLocaleDateString()}</span>
                          <span className="text-gray-700 mb-2 ps-3">{jezik.profilUgovori.dkraj}: {new Date(ugovor.datumZavrsetka).toLocaleDateString()}</span>
                        </p>
                        <p className="text-gray-700 mb-2">{jezik.pregledi.plata}: {ugovor.cenaPoSatu} EUR</p>
                    <p className="text-gray-700 mb-2 font-semibold">Status: {translateStatus(ugovor.status)}</p>
                    <div className='flex flex-wrap justify-center gap-5'>
                      <button onClick={() => downloadPDF(ugovor)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.download}</button>
                      {!ugovor.pripadaGrupi && (
                        <button onClick={() => handleAddRecenzija(ugovor.id, ugovor.majstorID, ugovor.poslodavacID)} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilUgovori.recenzija}</button>
                      )}
                    </div>
                  </div>
                )
              }
            })}
          </div>
        </div>
      )}
    </div>
  );
};