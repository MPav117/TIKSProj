import { useContext} from "react"
import { useNavigate } from "react-router-dom"
import { AppContext } from "../App"

export default function Oglas(props){
    const navigate = useNavigate()
    const korisnik = useContext(AppContext).korisnik
    const setOglas = useContext(AppContext).setOglas
    const jezik = useContext(AppContext).jezik

    const selectOglas = () => {
        setOglas(props.oglas)
        sessionStorage.setItem('oglas', props.oglas.idOglas)
        navigate('../view_job_posting')
    }

    const handlePrijava = async () => {
        const token = sessionStorage.getItem('jwt')
        try {
            const response = await fetch(`https://localhost:7080/Majstor/prijaviNaOglas?id=${props.oglas.idOglas}`, {
                method: 'POST',
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

    const handleOdjava = async () => {
        const token = sessionStorage.getItem('jwt')
        try {
            const response = await fetch(`https://localhost:7080/Majstor/OdjaviSaOglasa/${props.oglas.idOglas}`, {
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
        <div 
            onClick={selectOglas}
            className="border border-gray-300 rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow duration-300 cursor-pointer my-4 bg-[#fff7ed] flex justify-between">
            <div>
            <h2 className="font-semibold">{props.oglas.naslov}</h2>
            <p>{props.oglas.opis}</p>
            <p>{jezik.pregledi.plata}: {props.oglas.cenaPoSatu} EUR</p>
            <p className="font-semibold">{jezik.profilOglasi.vestine}: {translateVestine(props.oglas.listaVestina).join(', ')} </p>
            </div>
            {/* Dugme unutar diva */}
            <div className="flex justify-end items-end"onClick={(e) => e.stopPropagation()}>
            {korisnik !== null && korisnik.tip !== 'poslodavac' && props.oglas.prijavljeni !== null && !props.oglas.prijavljeni.includes(korisnik.id) && (
                <button onClick={handlePrijava} className="w-30 h-auto bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-3 py-2 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300">
                    {jezik.oglas.prijavi}
                </button>
            )}
            {korisnik !== null && korisnik.tip !== 'poslodavac' && props.oglas.prijavljeni !== null && props.oglas.prijavljeni.includes(korisnik.id) && (
                <button onClick={handleOdjava} className="w-30 h-auto  bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-3 py-2 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">
                    {jezik.oglas.odjavi}
                </button>
            )}
            </div>
        </div>
     );
}