import { BrowserRouter, Routes, Route } from 'react-router-dom';
import Header from './komponente/Header';
import Pocetna from './stranice/Pocetna';
import Profil from './stranice/Profil';
import PregledMajstora from './stranice/PregledMajstora';
import FormaProfil from './stranice/FormaProfil';
import { createContext, useEffect, useState } from 'react';
import ProfilHeader from './komponente/ProfilHeader';
import PregledOglasa from './stranice/PregledOglasa';
import FormaOglasZahtevPosao from './stranice/FormaOglasZahtevPosao';
import StranicaProfil from './stranice/StranicaProfil';
import StranicaOglas from './stranice/StranicaOglas';
import FormaUpdate from './stranice/FormaUpdate';
import FormaUgovor from './stranice/FormaUgovor';
import FormaZahtevGrupa from './stranice/FormaZahtevGrupa';
import FormaRecenzija from './stranice/FormaRecenzija';
import Chat from './komponente/Chat';
import Loader from './komponente/Loader';

const AppContext = createContext()

function App() {
  const [jezikID, setJezikID] = useState(sessionStorage.getItem('jezik') ?? 'en')
  const [jezik, setJezik] = useState(null)
  const [korisnik, setKorisnik] = useState(null)
  const [naProfilu, setNaProfilu] = useState(false)
  const [gledaniKorisnik, setGledaniKorisnik] = useState(null)
  const [oglas, setOglas] = useState(null)
  const [logovan, setLogovan] = useState(sessionStorage.getItem('jwt') !== null)

  useEffect(() => {
    // Ako je trenutna lokacija na stranici profila, postavite stanje na true
    setNaProfilu(window.location.pathname === '/profile');
    tryLoad()
    const j = sessionStorage.getItem('jezik')
    if (j !== null && j !== 'en'){
      loadLang(j)
    }
  }, [])

  useEffect(() => {
    if (jezik == null || jezik.id !== jezikID){
      loadLang(jezikID)
    }
    if (logovan && korisnik === null){
      tryLoad()
    }
  })

  const tryLoad = async () => {
    const token = sessionStorage.getItem('jwt')
    if (token !== null){
      try {
        const response = await fetch('https://localhost:7080/Profil/podaciRegistracije', {
            headers: {
              Authorization: `bearer ${token}`
            }
        })
        if (response.ok){
          const data = await response.json()
          setKorisnik(data)
          sessionStorage.setItem('mojiID', `${data.id}`)
          if (data.tip === 'majstor' && data.tipMajstora === 'grupa'){
            sessionStorage.setItem('mojiFlag', '1')
          }
        }
        else {
          window.alert(await response.text())
        }
      } catch (error) {
        window.alert(error.message)
      }
    }
  }

  const loadLang = async id => {
    if (id === undefined || id === null){
      return
    }

    try {
      const response = await fetch(`https://localhost:7080/Osnovni/PromeniJezik/${id}`)
      if (response.ok){
        const data = await response.json()
        setJezik(data)
        setJezikID(data.id)
      }
    } catch (error) {
        console.log(error)
        window.alert(error)
    }
  }

  if (jezik === null || (logovan && korisnik === null)){
    return <Loader />
  }

  return (
    <AppContext.Provider value={{jezik: jezik, setJezik: setJezik, korisnik: korisnik, setKorisnik: setKorisnik, setNaProfilu: setNaProfilu, 
        gledaniKorisnik: gledaniKorisnik, setGledaniKorisnik: setGledaniKorisnik, oglas: oglas, setOglas: setOglas, jezikID: jezikID, setJezikID: setJezikID, setLogovan: setLogovan}}>
      <BrowserRouter>
        {naProfilu ? <ProfilHeader /> : <Header />}
        <Routes>
          <Route exact path="/" element={<Pocetna />}/>
          <Route exact path="/profile" element={<Profil />}/>
          <Route exact path="/search_craftsmen" element={<PregledMajstora />}/>
          <Route exact path="/search_job_postings" element={<PregledOglasa />}/>
          <Route exact path="/login" element={<FormaProfil stanje={"login"} tip={'login'} grupa={false}/>}/>
          <Route exact path="/register_craftsman" element={<FormaProfil stanje={"register"} tip={"majstor"} grupa={false}/>}/>
          <Route exact path="/register_employer" element={<FormaProfil stanje={"register"} tip={"poslodavac"} grupa={false}/>}/>
          <Route exact path='/create_job_posting' element={<FormaOglasZahtevPosao stanje={'oglas'}/>}/>
          <Route exact path='/create_job_request' element={<FormaOglasZahtevPosao stanje={'zahtev'}/>}/>
          <Route exact path='/view_profile' element={<StranicaProfil profil={gledaniKorisnik}/>}/>
          <Route exact path='/view_job_posting' element={<StranicaOglas oglas={oglas}/>}/>
          <Route exact path='/edit-profile' element={<FormaUpdate />}/>
          <Route exact path='/ugovor' element={<FormaUgovor />}/>
          <Route exact path='/create_group_request' element={<FormaZahtevGrupa />}/>
          <Route exact path='/register_craftsman_group' element={<FormaProfil stanje={'register'} tip={'majstor'} grupa={true}/>}/>
          <Route exact path='/add_review' element={<FormaRecenzija />}/>
          <Route path="/chat" element={<Chat/>} />
        </Routes>
      </BrowserRouter>
    </AppContext.Provider>
  )
}

export default App
export { AppContext }
