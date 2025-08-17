import { useContext, useEffect, useState } from "react";
import { AppContext } from "../App";
import { useNavigate } from "react-router-dom";
import Kalendar from "../komponente/Kalendar";
import ListaProfilaMajstora from "../komponente/ListaProfilaMajstora";
import ListaRecenzija from "../komponente/ListaRecenzija";

export default function StranicaProfil(props){
    const [profil, setProfil] = useState({grad: ''})
    const [clanovi, setClanovi] = useState([])
    const [recenzije, setRecenzije] = useState([])
    const navigate = useNavigate()
    const korisnik = useContext(AppContext).korisnik
    const setGledaniKorisnik = useContext(AppContext).setGledaniKorisnik
    const [adminConfirm, setAdminConfirm] = useState(false)
    const jezik = useContext(AppContext).jezik
    const[pripada, setPripada]= useState(0)
    useEffect(() => {
        if (props.profil !== null){
            sessionStorage.setItem('view', `${props.profil.id}`)
        }
        loadProfil()
    }, [])

    const loadProfil = async () => {
      let id = sessionStorage.getItem('view')
      try {
          let response = await fetch(`https://localhost:7080/Profil/vratiKorisnika/${id}`)
          if (response.ok){
              let data = await response.json()
              setProfil(data)
              if (data.tip === 'majstor' && data.tipMajstora === 'grupa'){
                  response = await fetch(`https://localhost:7080/Majstor/GetClanovi/${id}`)
                  if (response.ok){
                      data = await response.json()
                      setClanovi(data)
                  }
                  else {
                      window.alert(jezik.general.error.netGreska + ": " + await response.text())
                  }
              }
  
              response = await fetch(`https://localhost:7080/Osnovni/GetRecenzije/${id}`)
              if (response.ok){
                  data = await response.json()
                  setRecenzije(data)
              }
              else {
                  window.alert(jezik.general.error.netGreska + ": " + await response.text())
              }
          }
          else {
              window.alert(jezik.general.error.netGreska + ": " + await response.text())
          }
      } catch (error) {
          window.alert(jezik.general.netGreska + ": " + error.message)
      }

      const flag = sessionStorage.getItem('mojiFlag')
      
      if (flag !== null){
          id = Number.parseInt(id)
          const myID = sessionStorage.getItem('mojiID')
          try {
              const response = await fetch(`https://localhost:7080/Majstor/GetClanovi/${myID}`)
                if (response.ok){
                    const data = await response.json()
                    data.forEach(element => {
                      if (element.id === id){
                          setPripada(1)
                          return
                      }
                    });
                }
                else {
                    window.alert(jezik.general.error.netGreska + ": " + await response.text())
                }
            } catch (error) {
                window.alert(jezik.general.error.netGreska + ": " + error.message)
            }
      }
  }
    const handleMajstorZahtevPosao = () => {
        setGledaniKorisnik(profil)
        navigate('../create_job_request')
    }

    const handleMajstorZahtevGrupa = () => {
        setGledaniKorisnik(profil)
        navigate('../create_group_request')
    }

    const handleAdminIzbrisiProfil = () => {
      setAdminConfirm(true)
    }

    const handleAdminIzbrisiProfilConfirm = async () => {
        const token = sessionStorage.getItem('jwt')
        const id = sessionStorage.getItem('view')
        try {
            const response = await fetch(`https://localhost:7080/Profil/izbrisiProfil/${id}`, {
                method: "DELETE",
                headers: {
                  'Content-Type': "application/json",
                  Authorization: `Bearer ${token}`
                }
            })
            if (response.ok){
                navigate('../profile')
            }
            else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch (error) {
            window.alert(jezik.general.netGreska + ": " + error.message)
        }
        setAdminConfirm(false)
    }

    const handleAdminIzbrisiProfilCancel = () => {
      setAdminConfirm(false)
    }

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

    return (
        <div className="bg-[#fed7aa] bg-opacity-60 min-h-screen">
        <div className="p-4 relative">
          <div className="flex flex-col lg:flex-row">
            <div className={`w-full ${profil.tip === 'majstor' && ('lg:w-2/3')} max-w-xxl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow flex relative flex-col sm:flex-row mb-4 md:mb-0`}>
              <div className="flex flex-col items-center w-1/3 p-4 min-w-56">
                
                <img className="w-48 h-48 mb-3 rounded-full shadow-lg" src={profil.slika || "/images/"} alt={jezik.header.profil} />
                <h5 className="mb-1 text-2xl font-medium text-gray-900">{profil.username}</h5>
                <span className="text-lg text-gray-500">{profil.tip}</span>
              </div>
              <div className="flex flex-col justify-center w-2/3 p-4 relative">
              <div className='h-0 sm:h-12'></div>
                {profil.tip === 'majstor' ? (
                  <div className="text-left">
                    <p className="text-xl text-gray-600 font-semibold text-wrap">{profil.naziv}</p>
                    <p className="text-lg text-gray-600 text-wrap break-all">{profil.opis}</p>
                    <p className="text-lg text-gray-600 text-wrap">{jezik.pregledi.ocena}: {renderStars(profil.prosecnaOcena)}</p>
                    <p className="pt-1 text-lg text-gray-600 font-semibold text-wrap">{jezik.formaProfil.vestine}: {translateVestine(profil.listaVestina).join(', ')}</p>
                    <p className="text-lg text-gray-600 text-wrap">Email: {profil.email}</p>
                    <p className="pt-5 text-lg text-gray-600 text-wrap">{jezik.stranicaProfil.iz} {profil.grad.city_ascii}, {profil.grad.country}</p>
                  </div>
                ) : (
                  <div className="text-left">
                    <p className="text-xl text-gray-600 font-semibold text-wrap">{profil.naziv}</p>
                    <p className="text-lg text-gray-600 text-wrap break-all">{profil.opis}</p>
                    <p className="text-lg text-gray-600 text-wrap">{jezik.pregledi.ocena}: {renderStars(profil.prosecnaOcena)}</p>
                    <p className="text-lg text-gray-600 text-wrap">Email: {profil.email}</p>
                    <p className="pt-5 text-lg text-gray-600 text-wrap">{jezik.formaProfil.adresa}: {profil.adresa}, {profil.grad.city_ascii}, {profil.grad.country}</p>
                  </div>
                )}
                
                
              </div>
              <div className="absolute top-0 right-0 mt-2 mr-2 flex flex-col sm:flex-row-reverse gap-3">
                {(korisnik !== null && korisnik.tip === 'poslodavac' && profil.tip === 'majstor') && (
                    <button onClick={handleMajstorZahtevPosao} className=" bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profil.zahtevPosao}</button>
                )}
                {(pripada === 0 && korisnik !== null && korisnik.tip === 'majstor' && korisnik.grupa === 0 && profil.tip === 'majstor' && profil.id !== korisnik.id && profil.tipMajstora === 'majstor') && (
                    <button onClick={handleMajstorZahtevGrupa} className=" bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profil.zahtevGrupa}</button>
                )}
                {(korisnik !== null && korisnik.jeAdmin && profil.id !== korisnik.id) && (
                    <button onClick={handleAdminIzbrisiProfil} className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-3 py-2 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">{jezik.profil.adminIzbrisi}</button>
                )}
                </div>
            </div>


          {profil.tip === 'majstor' && (
                      <div className="w-full lg:w-1/3 max-w-xxl h-auto  rounded-lg  ml-4 pt-3 lg:pt-0">
                          <Kalendar licniProfil={false} id={profil.id }/>
                      </div>
                  )}
            </div>

          {clanovi !== null && clanovi.length>0 &&(
                <ListaProfilaMajstora lista={clanovi}/>
          )}       
          {(recenzije != null && recenzije.length != null && recenzije.length > 0) && (
            <ListaRecenzija lista={recenzije} id={profil.id}/>
          )}
        </div>
          {adminConfirm && (
            <div className="fixed inset-0 flex items-center justify-center">
                <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
                <div className="bg-white p-4 rounded-lg shadow-lg w-80 relative border border-yellow-600">
                <p className="mb-4 text-center">{jezik.profil.prompt}</p>
                <div className="flex justify-end space-x-4">
                    <button onClick={handleAdminIzbrisiProfilConfirm} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
                    <button onClick={handleAdminIzbrisiProfilCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
                </div>
                </div>
            </div>
          )}
        </div>
    ); 
}