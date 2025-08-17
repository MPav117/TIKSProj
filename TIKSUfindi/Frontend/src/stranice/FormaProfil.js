import { useContext, useEffect, useState } from "react"
import { Link, useNavigate } from "react-router-dom"
import { AppContext } from "../App"

export default function FormaProfil(props){

    const [inputs, setInputs] = useState({username: "", password: "", naziv: "", slika: "", opis: "", email: "", adresa: "", grad: ""})
    const [vestine, setVestine] = useState([])
    const [gradovi, setGradovi] = useState([])
    const [grad, setGrad] = useState(null)
    const navigate = useNavigate()
    const [povezan, setPovezan] = useState(0)
    const setNaProfilu = useContext(AppContext).setNaProfilu
    const korisnik = useContext(AppContext).korisnik
    const setKorisnik = useContext(AppContext).setKorisnik
    const jezik = useContext(AppContext).jezik

    useEffect(() => {
      const p = sessionStorage.getItem('povezani')
      if (korisnik !== null){
        if (!props.grupa){
          setInputs({...korisnik, ["username"]: "", ["password"]: "", ["adresa"]: "" })
        }
        if (props.stanje !== 'login'){
          setGrad(document.getElementById('stdgrad').value)
        }
      }
      if (p !== null){
        setPovezan(Number.parseInt(p))
      }
      else {
        setPovezan(0)
      }
    }, [])

    const handleGradChange = async () => {
      if (inputs.grad !== ""){
        try {
          let response = await fetch(`https://localhost:7080/Profil/GetGradovi?start=${inputs.grad}`)
  
          if (response.ok){
            response = await response.json()
            setGradovi(response)
            if (response.length > 0 && povezan === 0){
              setGrad(response[0].id)
            }
          }
          else {
            window.alert(jezik.general.error.netGreska + ": " + await response.text())
          }
        } catch (error) {
          window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
      }
    }

    const handleGradSelect = e => {
      setGrad(e.target.value)
    }

    const handleSubmit = async e => {
        e.preventDefault()
        
        if (props.stanje === 'login'){
          if (inputs.username === "" || inputs.password === ""){
            window.alert(jezik.general.error.nepunaForma)
            return
          }
        }
        else if (props.tip === 'majstor'){
          if (inputs.username === "" || inputs.password === "" || inputs.naziv === "" || inputs.slika === "" || inputs.opis === "" || inputs.email === "" || vestine === null || grad === null){
            window.alert(jezik.general.error.nepunaForma)
            return
          }
        }
        else if (props.tip === 'poslodavac'){
          if (inputs.username === "" || inputs.password === "" || inputs.naziv === "" || inputs.slika === "" || inputs.opis === "" || inputs.email === "" || inputs.adresa === "" || grad === null){
            window.alert(jezik.general.error.nepunaForma)
            return
          }
        }
        
        
        if (props.stanje === "login"){
          try {
            const response = await fetch('https://localhost:7080/Profil/login', {
                method: "POST",
                headers: {'Content-Type': "application/json"},
                credentials: 'include',
                body: JSON.stringify({
                    username: inputs.username,
                    password: inputs.password
                })
            })
  
            if (response.ok){
                setNaProfilu(true)
                const token= await response.text();
                sessionStorage.setItem("jwt", token);
                sessionStorage.removeItem('povezani')
                setKorisnik(null)
                navigate("../profile")
            }
            else{
              window.alert(jezik.general.error.badRequest)
            }
          } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
          }
        }
        else if (props.tip === "majstor"){
          let response = null
          try {
            if (!props.grupa){
              response = await fetch('https://localhost:7080/Profil/register-majstor', {
                  method: "POST",
                  headers: {'Content-Type': "application/json"},
                  body: JSON.stringify({
                      username: inputs.username,
                      password: inputs.password,
                      tip: "majstor",
                      naziv: inputs.naziv,
                      slika: inputs.slika,
                      opis: inputs.opis,
                      gradID: grad,
                      email: inputs.email,
                      tipMajstora: "majstor",
                      listaVestina: vestine,
                      povezani: povezan
                  })
              })
            }
            else {
              const token = sessionStorage.getItem('jwt')
              response = await fetch(`https://localhost:7080/Profil/register-grupaMajstora`, {
                  method: "POST",
                  headers: {
                    'Content-Type': "application/json",
                    Authorization: `Bearer ${token}`
                  },
                  body: JSON.stringify({
                      username: inputs.username,
                      password: inputs.password,
                      tip: "majstor",
                      naziv: inputs.naziv,
                      slika: inputs.slika,
                      opis: inputs.opis,
                      gradID: grad,
                      email: inputs.email,
                      tipMajstora: "grupa",
                      listaVestina: vestine,
                      povezani: 0
                  })
              })
            }
  
            if (response.ok){
              sessionStorage.removeItem('povezani')
              navigate("../login")
            }
            else{
              window.alert(jezik.general.error.badRequest)
            }
          } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
          }
        }
        else{
          try {
            const response = await fetch('https://localhost:7080/Profil/register-poslodavac', {
                method: "POST",
                headers: {'Content-Type': "application/json"},
                body: JSON.stringify({
                    username: inputs.username,
                    password: inputs.password,
                    tip: "poslodavac",
                    naziv: inputs.naziv,
                    slika: inputs.slika,
                    opis: inputs.opis,
                    gradID: grad,
                    adresa: inputs.adresa,
                    email: inputs.email,
                    povezani: povezan
                })
            })
  
            if (response.ok){
              sessionStorage.removeItem('povezani')
              navigate("../login")
            }
            else{
              window.alert(jezik.general.error.badRequest)
            }
          } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
          }
        }
    }

    const handleInputChange = e => {
        setInputs({...inputs, [e.target.name]: e.target.value})
    }

    const handleImageChange = () => {
        const el = document.getElementById("pfp")
        const img = el.files[0]
        const reader = new FileReader()
        reader.onloadend = () => {
            setInputs({...inputs, ["slika"]: reader.result})
          }
        reader.readAsDataURL(img)
    }

    const handleOpisChange = () => {
        const textOpis = document.getElementById("opis")
        setInputs({...inputs, ["opis"]: textOpis.value})
    }

    const handleVestineChange = () => {
        const vestineBoxes = document.querySelectorAll(".vestine")
        let newVestine = []
        vestineBoxes.forEach(v => {
            if (v.checked){
                newVestine.push(v.value)
            }
        })
        const drugeVestine = document.getElementById("drugo").value.split(", ")
        drugeVestine.forEach((v) => {
            if (v.length >= 2){
                newVestine.push(v)
            }
        })
        setVestine(newVestine)
    }

    return (
      <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible">
        <div className="h-screen"></div>
        <div className="bg-white bg-opacity-70 shadow-lg p-8 w-full md:w-2/3 lg:w-1/2 xl:w-1/3 rounded-lg backdrop-blur-sm">  
          <form onSubmit={handleSubmit}>
            <div className="flex flex-col space-y-4">
              <div className="w-full">
                
                <label htmlFor="username" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.username}*</label>
                <input
                  name="username"
                  type="text"
                  placeholder={jezik.formaProfil.username}
                  onChange={handleInputChange}
                  required
                  className={`p-3 border rounded-lg w-full ${inputs.username ==='' ? "border-red-400" : "border-gray-100"}`}
                />
              </div>
              <div className="w-full">
                <label htmlFor="password" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.password}*</label>
                <input
                  name="password"
                  type="password"
                  placeholder={jezik.formaProfil.password}
                  onChange={handleInputChange}
                  required
                  className={`p-3 border rounded-lg w-full ${inputs.password ==='' ? "border-red-400" : "border-gray-100"}`}
                />
              </div>
              
              {props.stanje === 'login' && (
                <>
                  <button
                    type="submit"
                    className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300"  
                  >
                    {jezik.formaProfil.login}
                  </button>
                </>
              )}
              
              {props.stanje !== 'login' && (
                <>
                  <div className={`w-full flex-row relative ${inputs.slika !== '' && 'h-28'} align-middle`}>
                    <label htmlFor="pfp" className="block text-gray-700 font-bold mb-2 w-max">{jezik.formaProfil.slika}*</label>
                    {inputs.slika !== '' && (
                      <span className="w-20 absolute left-0">
                        <img src={inputs.slika} alt={jezik.formaProfil.slika} className="h-20 w-20 object-cover rounded-full" />
                      </span>
                    )}
                    <input
                      id="pfp"
                      type="file"
                      accept=".jpg, .jpeg, .png" 
                      onChange={handleImageChange}
                      className={`p-3 border rounded-lg ${inputs.slika === '' ? 'w-full border-red-400 ' : 'w-1/2 absolute left-24'}`}
                    />
                  </div>
                  <div className="w-full">
                    <label htmlFor="naziv" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.naziv}*</label>
                    <input
                      name="naziv"
                      type="text"
                      placeholder={jezik.formaProfil.naziv}
                      value={inputs.naziv}
                      onChange={handleInputChange}
                      className={`p-3 border rounded-lg w-full ${inputs.naziv ==='' ? "border-red-400" : "border-gray-100"}`}
                    />
                  </div>
                  <div className="w-full">
                    <label htmlFor="opis" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.opis}*</label>
                    <textarea
                      id="opis"
                      placeholder={jezik.formaProfil.opis}
                      value={inputs.opis}
                      onChange={handleOpisChange}
                      className={`p-3 border rounded-lg w-full ${inputs.opis ==='' ? "border-red-400" : "border-gray-100"}`}
                    />
                  </div>
                  <div className="w-full">
                    <label htmlFor="email" className="block text-gray-700 font-bold mb-2 w-full">Email*</label>
                    <input
                      name="email"
                      type="email"
                      placeholder="Email"
                      value={inputs.email}
                      onChange={handleInputChange}
                      className={`p-3 border rounded-lg w-full ${inputs.email ==='' ? "border-red-400" : "border-gray-100"}`}
                    />
                  </div>
                  <div className="w-full">
                    <label htmlFor="grad" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.grad}*</label>
                    <div className="flex flex-row gap-2">
                      <input name="grad" type="text" onChange={handleInputChange} placeholder={jezik.formaProfil.grad} className={`p-3 border rounded-lg w-1/2 ${grad === null ? "border-red-400" : "border-gray-100"}`}></input>
                      <select name="grad" onChange={handleGradSelect} className="p-3 border rounded-lg w-1/2">
                        {korisnik !== null && (
                          <option id="stdgrad" value={korisnik.gradID}>{korisnik.city_ascii}, {korisnik.country}</option>
                        )}
                        {gradovi.map(grad => (
                          <option key={grad.id} value={grad.id}>{grad.city_ascii}, {grad.country}</option>
                        ))}
                      </select>
                      <input type="button" value={jezik.pregledi.search} onClick={handleGradChange} disabled={inputs.grad == ""} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300" ></input>
                    </div>
                  </div>
                </>
              )}

              {props.tip === 'majstor' && (
                <>
                  <div className="w-full">
                    <label htmlFor="vestine" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.vestine}*</label>
                    <div className="flex flex-wrap justify-center pb-3">
                      <div className="flex flex-wrap">
                        <div>
                          <input
                            type="checkbox"
                            value="stolar"
                            className="vestine"
                            onChange={handleVestineChange}
                          />
                          <label htmlFor="stolar" className="ps-3 pe-5">{jezik.formaProfil.stolar}</label>
                        </div>
                        <div>
                          <input
                            type="checkbox"
                            value="elektricar"
                            className="vestine"
                            onChange={handleVestineChange}
                          />
                          <label htmlFor="elektricar" className="ps-3 pe-5">{jezik.formaProfil.elektricar}</label>
                        </div>
                      </div>
                      <div className="flex flex-wrap">
                        <div>
                          <input
                            type="checkbox"
                            value="vodoinstalater"
                            className="vestine"
                            onChange={handleVestineChange}
                          />
                          <label htmlFor="vodoinstalater" className="ps-3 pe-5">{jezik.formaProfil.vodoinstalater}</label>
                        </div>
                        <div>
                          <input
                            type="checkbox"
                            value="keramicar"
                            className="vestine"
                            onChange={handleVestineChange}
                          />
                          <label htmlFor="keramicar" className="ps-3 pe-5">{jezik.formaProfil.keramicar}</label>
                        </div>
                      </div>
                    </div>
        
                    <input
                      name="drugo"
                      type="text"
                      placeholder={jezik.formaProfil.drugo}
                      id="drugo"
                      onChange={handleVestineChange}
                      className="p-3 border rounded-lg w-full"
                    />
                  </div>
                  
                  <button
                    type="submit"
                    className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300"
                  >
                    {jezik.formaProfil.register}
                  </button>

                  <div className="text-center mt-4">  
                      <Link to="/login" className="text-blue-500 hover:underline">  
                          {jezik.formaProfil.loginPrompt}
                      </Link>
                  </div>
                </>
              )}

              {props.tip === 'poslodavac' && (
                <>
                  <div className="w-full">
                    <label htmlFor="adresa" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.adresa}*</label>
                    <input
                      name="adresa"
                      type="text"
                      placeholder={jezik.formaProfil.adresa}
                      onChange={handleInputChange}
                      className={`p-3 border rounded-lg w-full ${inputs.adresa ==='' ? "border-red-400" : "border-gray-100"}`}
                    />
                  </div>
                  
                  <button
                    type="submit"
                    className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300"
                  >
                    {jezik.formaProfil.register}
                  </button>

                  <div className="text-center mt-4">  
                      <Link to="/login" className="text-blue-800 hover:underline">  
                          {jezik.formaProfil.loginPrompt}
                      </Link>
                  </div>
                </>
              )}
            </div>
          </form>
        </div>
      </div>
    )
}