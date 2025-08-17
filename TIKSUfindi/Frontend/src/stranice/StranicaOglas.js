import { useContext, useEffect, useState } from "react";
import { AppContext } from "../App";
import ListaProfilaMajstora from "../komponente/ListaProfilaMajstora";
import Loader from "../komponente/Loader";
import { useNavigate } from "react-router-dom";

export default function StranicaOglas(props){

    const [oglas, setOglas] = useState({slike: [], prijavljeni: []})
    const [poslodavac, setPoslodavac] = useState({})
    const [profili, setProfili] = useState([])
    const [korisnik, setKorisnik] = useState(null)
    const [loading, setLoading] = useState(true)
    const [isOglasVisible, setIsOglasVisible] = useState(false);
    const navigate = useNavigate()
    const jezik = useContext(AppContext).jezik
    const setGledaniKorisnik = useContext(AppContext).setGledaniKorisnik

    const [currentImageIndex, setCurrentImageIndex] = useState(0);

    const toggleOglasVisibility = () => {
        setIsOglasVisible(!isOglasVisible);
      };

    const handlePreviousImage = () => {
    setCurrentImageIndex((prevIndex) => (prevIndex === 0 ? oglas.slike.length - 1 : prevIndex - 1));
    };

    const handleNextImage = () => {
    setCurrentImageIndex((prevIndex) => (prevIndex === oglas.slike.length - 1 ? 0 : prevIndex + 1));
    };

    useEffect(() => {
        if (props.oglas !== null){
            if (props.oglas.id){
                sessionStorage.setItem('oglas', `${props.oglas.id}`)
            }
            else {
                sessionStorage.setItem('oglas', `${props.oglas.idOglas}`)
            }
        }
        loadOglas()
    }, [])

    const formatDate = (dateString) => {
        const date = new Date(dateString);
        const month = date.getMonth() + 1; 
        const day = date.getDate();
        const year = date.getFullYear();
        return `${month}/${day}/${year}`;
      };

    const loadOglas = async () => {
        let korzaload = korisnik
        const token = sessionStorage.getItem('jwt')
        if (token !== null){
        try {
            const response = await fetch('https://localhost:7080/Profil/podaciRegistracije', {
                headers: {
                Authorization: `bearer ${token}`
                }
            })
            if (response.ok){
                korzaload = await response.json()
                setKorisnik(korzaload)
            }
            else {
            window.alert(await response.text())
            }
        } catch (error) {
            window.alert(error.message)
        }
        }
        const oglasId = sessionStorage.getItem('oglas')
        try {
            const response = await fetch(`https://localhost:7080/Osnovni/GetOglas/${oglasId}`)
            if (response.ok){
                const data = await response.json()
                setOglas(data)
                const response2 = await fetch(`https://localhost:7080/Profil/vratiKorisnika/${data.idKorisnik}`)
                if (response2.ok){
                    setPoslodavac(await response2.json())
                    if (data.prijavljeni !== null && korzaload !== null && data.idKorisnik === korzaload.id){
                        const prijavljeniMajstori = []
                         await data.prijavljeni.forEach(async id => {
                            const response3 = await fetch(`https://localhost:7080/Profil/vratiKorisnika/${id}`)
            
                            if (response3.ok){
                                let newMajstor = await response3.json()
                                newMajstor = {...newMajstor, ['id']: id}
                                prijavljeniMajstori.push(newMajstor)
                            }
                            else {
                                window.alert(jezik.general.error.netGreska + ": " + response3.body)
                            }
                        })
                        setProfili(prijavljeniMajstori)
                    }
                    setTimeout(() => {
                        setLoading(false)
                    }, 1000);
                }
                else {
                    window.alert(jezik.general.error.netGreska + ": " + response2.body)
                }
            }
            else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
    }

    const handlePrijava = async () => {
        const token = sessionStorage.getItem('jwt')
        try {
            const response = await fetch(`https://localhost:7080/Majstor/prijaviNaOglas?id=${oglas.id}`, {
                method: 'POST',
                headers: {
                    Authorization: `bearer ${token}`
                  }
            })
            if (response.ok){
                loadOglas()
            }
            else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)   
        }
    }

    const handleOdjava = async () => {
        const token = sessionStorage.getItem('jwt')
        try {
            const response = await fetch(`https://localhost:7080/Majstor/OdjaviSaOglasa/${oglas.id}`, {
                method: 'DELETE',
                headers: {
                    Authorization: `bearer ${token}`
                  }
            })
            if (response.ok){
                loadOglas()
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

      const handleViewKorisnik = async () => {
        try {
            const response = await fetch(`https://localhost:7080/Profil/vratiKorisnika/${oglas.idKorisnik}`)
            if (response.ok){
                const data = await response.json()
                sessionStorage.setItem('view', `${oglas.idKorisnik}`)
                setGledaniKorisnik({...data, ['id']: oglas.idKorisnik})
                navigate(`../view_profile`)
            }
            else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch(error){
            window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
      }

    if (loading){
        return <Loader />
    }
    return (
        <>
            <div className="flex flex-col items-center bg-orange-200 bg-opacity-60 min-h-screen p-4">
                <div className="bg-white rounded-lg p-4 w-full max-w-screen-lg mx-auto">
                    <p className="text-center text-2xl font-semibold mt-9 mb-3 text-wrap">
                        {oglas.naslov.toUpperCase()}
                    </p>
                    <p className="text-center text-xl mb-2 text-wrap" onClick={handleViewKorisnik}>
                        {oglas.naziv}
                    </p>
                    <div className="w-full border border-orange-200 rounded-2xl shadow bg-[#fff7ed] mx-auto mt-4 flex flex-wrap p-4">
                        <div className="w-full p-4">
                            <div className="flex flex-col justify-center">
                                <p className="text-lg mb-2 text-center text-wrap">{oglas.opis}</p>
                                <p className="text-lg mb-2 text-center">{jezik.formaUgovor.datumk}: {formatDate(oglas.datumZavrsetka)}</p>
                                <p className="text-lg mb-5 text-center">{jezik.pregledi.plata}: {oglas.cenaPoSatu} EUR</p>
                                <p className="text-lg font-semibold text-center">
                                    {jezik.profilOglasi.vestine}: {translateVestine(oglas.listaVestina).join(', ')}
                                </p>
                            </div>
                        </div>
                        <div className="w-full flex justify-center">
                            {korisnik !== null && korisnik.tip === 'majstor' && oglas.prijavljeni !== null && !oglas.prijavljeni.includes(korisnik.id) && (
                                <button
                                    onClick={handlePrijava}
                                    className="bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-3 py-2 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300"
                                >
                                    {jezik.oglas.prijavi}
                                </button>
                            )}
                            {korisnik !== null && korisnik.tip === 'majstor' && oglas.prijavljeni !== null && oglas.prijavljeni.includes(korisnik.id) && (
                                <button
                                    onClick={handleOdjava}
                                    className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-3 py-2 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300"
                                >
                                    {jezik.oglas.odjavi}
                                </button>
                            )}
                        </div>
    
                        {oglas.slike !== null && (
                            <div className="w-full p-3">
                                <div className="relative flex justify-center">
                                    <img className="max-w-full w-auto max-h-96 p-2 shadow-lg" src={oglas.slike[currentImageIndex] || "/images/"} alt="slike" />
                                    {oglas.slike.length > 1 && (
                                        <>
                                            <button
                                                className="absolute top-1/2 left-4 transform -translate-y-1/2 bg-gray-200 rounded-full p-2 opacity-50"
                                                onClick={handlePreviousImage}
                                            >
                                                &lt;
                                            </button>
                                            <button
                                                className="absolute top-1/2 right-4 transform -translate-y-1/2 bg-gray-200 rounded-full p-2 opacity-50"
                                                onClick={handleNextImage}
                                            >
                                                &gt;
                                            </button>
                                        </>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                    {korisnik !== null && korisnik.tip === 'poslodavac' && korisnik.id===oglas.idKorisnik && oglas.prijavljeni.length > 0 && (
                        <div className="flex justify-center items-center">
                            <button
                                className="w-full max-w-xs h-16 bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300 mb-4 mt-6"
                                onClick={toggleOglasVisibility}
                            >
                                {jezik.oglas.prijavljeni}
                            </button>
                        </div>
                    )}
                    {isOglasVisible && (
                        <div className="w-full max-w-screen-lg rounded-lg shadow mx-auto mt-4 bg-white">
                            <ListaProfilaMajstora lista={profili} idPoslodavca={oglas.idKorisnik} oglas={oglas} />
                        </div>
                    )}
                </div>
            </div>
        </>
    );
}