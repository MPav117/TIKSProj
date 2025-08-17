import { useContext, useState } from "react"
import { useLocation, useNavigate } from "react-router-dom"
import { AppContext } from "../App"

export default function ProfilMajstora(props){

    const [isRemoveConfirmOpen, setIsRemoveConfirmOpen] = useState(false)
    const navigate = useNavigate()
    const korisnik = useContext(AppContext).korisnik
    const setGledaniKorisnik = useContext(AppContext).setGledaniKorisnik
    const setOglas = useContext(AppContext).setOglas
    const location = useLocation()
    const jezik = useContext(AppContext).jezik

    const handleMajstorSelect = () => {
        setGledaniKorisnik(props.majstor)
        sessionStorage.setItem('view', `${props.majstor.id}`)
        navigate(`../view_profile`)
    }

    const handleMajstorZahtevPosao = () => {
        setGledaniKorisnik(props.majstor)
        sessionStorage.setItem('view', `${props.majstor.id}`)
        navigate('../create_job_request')
    }

    const handleSelectMajstorZaPosao = async () => {
        setOglas(props.oglas)
        sessionStorage.setItem('oglas', `${props.oglas.id}`)
        sessionStorage.setItem('view', `${props.majstor.id}`)
        navigate('../create_job_request')
    }

    const handleMajstorZahtevGrupa = async () => {
        setGledaniKorisnik(props.majstor)
        sessionStorage.setItem('view', `${props.majstor.id}`)
        navigate('../create_group_request')
    }

    const handleIzbaciIzGrupe = () => {
        setIsRemoveConfirmOpen(true)
      };

    const handleIzbaciIzGrupeConfirm = async () => {
        const token = sessionStorage.getItem('jwt')
        try {
            const response = await fetch(`https://localhost:7080/Majstor/IzbaciIzGrupe/${props.majstor.id}`, {
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
        setIsRemoveConfirmOpen(false)
    }

    const handleIzbaciIzGrupeCancel = () => {
        setIsRemoveConfirmOpen(false);
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
        <div className="border border-gray-300 rounded-lg p-4 shadow-md hover:shadow-lg transition-shadow duration-300 cursor-pointer my-4 bg-[#fff7ed]">
            <div className="flex items-center flex-wrap">
                <img src={`${props.majstor.slika}`} alt="Slika majstora" className="w-16 h-16 rounded-full mr-4" />
                <div className="pb-2">
                    <h1 className="font-bold">{props.majstor.naziv}</h1>
                    <p>{jezik.formaProfil.vestine}: {translateVestine(props.majstor.listaVestina).join(', ')}</p>
                    <p>{jezik.pregledi.ocena}: {renderStars(props.majstor.prosecnaOcena)}</p>
                    <p>{jezik.stranicaProfil.iz} {props.majstor.grad.city_ascii}, {props.majstor.grad.country}</p>
                </div>
            </div>
            <div className="flex justify-end">
                <div className="flex flex-wrap justify-center items-center space-x-2 space-y-2 md:space-y-0">
                    <button onClick={handleMajstorSelect}  className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilMajstora.view}</button>
                    {(korisnik !== null && korisnik.tip === 'poslodavac' && location.pathname !== '/view_job_posting') && (
                        <button onClick={handleMajstorZahtevPosao}  className="bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-3 py-2 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300">{jezik.profilMajstora.zahtevp}</button>
                    )}
                    {(korisnik !== null && korisnik.tip === 'majstor' && props.majstor.id !== korisnik.id && props.majstor.tipMajstora === 'majstor') && (
                        <button onClick={handleMajstorZahtevGrupa} className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.profilMajstora.zahtevg}</button>
                    )}
                    {(location.pathname === '/view_job_posting' && props.idPoslodavca !== null && props.idPoslodavca === korisnik.id) && (
                        <button onClick={handleSelectMajstorZaPosao} className="bg-green-700 bg-opacity-50 text-green-800 font-semibold border border-green-800 rounded-lg px-3 py-2 hover:bg-green-600 hover:border-green-700 hover:text-white transition-colors duration-300">{jezik.profilMajstora.izaberi}</button>
                    )}
                    {(location.pathname === '/profile' && props.count > 2) && (
                        <button onClick={handleIzbaciIzGrupe} className="bg-orange-700 bg-opacity-50 text-orange-800 font-semibold border border-orange-800 rounded-lg px-3 py-2 hover:bg-orange-600 hover:border-orange-700 hover:text-white transition-colors duration-300">{jezik.profilMajstora.izbaci}</button>
                    )}
                    {isRemoveConfirmOpen && (
                    <div className="fixed inset-0 flex items-center justify-center">
                        <div className="fixed inset-0 bg-orange-100 bg-opacity-75"></div>
                        <div className="bg-white p-4 rounded-lg shadow-lg w-80 relative border border-yellow-600">
                        <p className="mb-4 text-center">{jezik.profilMajstora.prompt}</p>
                        <div className="flex justify-end space-x-4">
                            <button onClick={handleIzbaciIzGrupeConfirm} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.yes}</button>
                            <button onClick={handleIzbaciIzGrupeCancel} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300">{jezik.general.no}</button>
                        </div>
                        </div>
                    </div>
                    )}
                </div>
            </div>
        </div>
    )
}