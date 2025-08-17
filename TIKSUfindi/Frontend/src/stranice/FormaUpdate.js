
import React, { useState, useEffect, useContext } from 'react';
import { useNavigate } from 'react-router-dom';
import { AppContext } from "../App";
import Loader from '../komponente/Loader';

export default function FormaUpdate() {
  const [formData, setFormData] = useState({
    naziv: '',
    adresa: '',
    slika: '',
    opis: '',
    gradID: '',
    email: '',
    vestine: []
  });
  //const [vestine, setVestine] = useState([]);
  const [gradovi, setGradovi] = useState([]);
  const [grad, setGrad] = useState(null);
  const navigate = useNavigate();
  const { korisnik, setKorisnik } = useContext(AppContext);
  const [vestineChecked, setVestineChecked]= useState();
  const jezik = useContext(AppContext).jezik
  const [gradTxt, setGradTxt] = useState('')

  useEffect(() => {
    if (korisnik) {
      setFormData({
        naziv: korisnik.naziv || '',
        adresa: korisnik.adresa || '',
        slika: korisnik.slika || '',
        opis: korisnik.opis || '',
        gradID: korisnik.gradID || '',
        email: korisnik.email || '',
        vestine: korisnik.listaVestina || []
      });
      setGrad(korisnik.gradID)
      const newVestineChecked = { ...vestineChecked };
      if (korisnik.tip !== 'poslodavac'){
        korisnik.listaVestina.forEach(vestina => {
          newVestineChecked[vestina] = true;
        });
        setVestineChecked(newVestineChecked);
      }
    }
  }, [korisnik]);

  const handleGradChange = async () => {
    if (gradTxt !== "") {
      try {
        const response = await fetch(
          `https://localhost:7080/Profil/GetGradovi?start=${gradTxt}`
        );
        if (response.ok){
          const data = await response.json();
          const gradoviArray = data || [];
          if (Array.isArray(gradoviArray)) {
            setGradovi(gradoviArray);
            setGrad(gradoviArray[0]?.id || null);
          } else {
            setGradovi([]);
          }
        }
        else {
          window.alert(jezik.general.error.netGreska + ": " + await response.text())
        }
      } catch (error) {
        window.alert(jezik.general.error.netGreska + ": " + error.message)
        setGradovi([]);
      }
    }
  };

  const handleGradSelect = (e) => {
    setGrad(e.target.value);
    console.log(e.target.value)
  };

  const handleGradTxtChange = e => {
    setGradTxt(e.target.value)
  }

  const handleImageChange = (e) => {
    const img = e.target.files[0];
    const reader = new FileReader();
    reader.onloadend = () => {
      setFormData(prevState => ({ ...prevState, slika: reader.result }));
    };
    reader.readAsDataURL(img);
  };
  const handleInputChange = (e) => {
    const { name, value } = e.target;
    setFormData(prevState => ({ ...prevState, [name]: value }));
  };

  const handleOpisChange = () => {
    const textOpis = document.getElementById("opis")
    setFormData({...formData, ["opis"]: textOpis.value})
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
    setFormData({...formData, vestine: newVestine})
}

  const handleSubmit = async (e) => {
    e.preventDefault();

    const token = sessionStorage.getItem("jwt");

    const url = korisnik.tip === "poslodavac"
      ? 'https://localhost:7080/Poslodavac/AzurirajPoslodavac2'
      : 'https://localhost:7080/Majstor/AzurirajMajstor2';

    const requestBody = {
      username: '',
      password: '',
      naziv: formData.naziv,
      slika: formData.slika,
      opis: formData.opis,
      gradID: grad,
      adresa: korisnik.tip === "poslodavac" ? formData.adresa : null,
      email: '',
      listaVestina: formData.vestine,
      tip: korisnik.tip,
      tipMajstora: korisnik.tip === "majstor" ? korisnik.tipMajstora : null,
      povezani: korisnik.povezani
    };

    console.log(requestBody)

    try {
      const response = await fetch(url, {
        method: 'PUT',
        headers: {
          'Content-Type': 'application/json',
          Authorization: `bearer ${token}`
        },
        body: JSON.stringify(requestBody)
      });

      if (response.ok) {
        setKorisnik({ ...korisnik, ...formData, listaVestina: formData.vestine });
        navigate("../profile");
      } else {
        window.alert(jezik.general.error.badRequest)
      }
    } catch (error) {
      window.alert(jezik.general.error.netGreska + ": " + error.message)
    }
  };

  if (!korisnik) {
    return <Loader />
  }

  return (
    <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible">
        <div className="h-screen"></div>
        <div className="bg-white bg-opacity-70 shadow-lg p-8 w-full md:w-2/3 lg:w-1/2 xl:w-1/3 rounded-lg backdrop-blur-sm">  
          <form onSubmit={handleSubmit}>
            <div className="flex flex-col space-y-4">

              <div className={`w-full flex-row relative ${formData.slika !== '' && 'h-28'} align-middle`}>
                <label htmlFor="pfp" className="block text-gray-700 font-bold mb-2 w-max">{jezik.formaProfil.slika}</label>
                {formData.slika !== null && formData.slika !== '' && (
                  <span className="w-20 absolute left-0">
                    <img src={formData.slika} alt={jezik.formaProfil.slika} className="h-20 w-20 object-cover rounded-full" />
                  </span>
                )}
                <input
                  id="pfp"
                  type="file"
                  accept=".jpg, .jpeg, .png"
                  onChange={handleImageChange}
                  className={`p-3 border rounded-lg ${formData.slika === '' ? 'w-full' : 'w-1/2 absolute left-24'}`}
                />
              </div>

              <div className="w-full">
                <label htmlFor="naziv" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.naziv}</label>
                <input
                  name="naziv"
                  type="text"
                  placeholder={jezik.formaProfil.naziv}
                  value={formData.naziv}
                  onChange={handleInputChange}
                  className="p-3 border rounded-lg w-full"
                />
              </div>

              <div className="w-full">
                <label htmlFor="opis" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.opis}</label>
                <textarea
                  id='opis'
                  placeholder={jezik.formaProfil.opis}
                  value={formData.opis}
                  onChange={handleOpisChange}
                  className="p-3 border rounded-lg w-full"
                />
              </div>

              <div className="w-full">
                <label htmlFor="grad" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.grad}</label>
                <div className="flex flex-row gap-2">
                  <input type="text" onChange={handleGradTxtChange} placeholder={jezik.formaProfil.grad} className="p-3 border rounded-lg w-1/2"></input>
                  <select name="grad" onChange={handleGradSelect} className="p-3 border rounded-lg w-1/2">
                    {korisnik !== null && (
                      <option id="stdgrad" value={korisnik.gradID}>{korisnik.city_ascii}, {korisnik.country}</option>
                    )}
                    {gradovi.map(grad => (
                      <option key={grad.id} value={grad.id}>{grad.city_ascii}, {grad.country}</option>
                    ))}
                  </select>
                  <input type="button" value={jezik.pregledi.search} onClick={handleGradChange} disabled={gradTxt == ""} className="px-4 py-2 bg-orange-200 text-black rounded hover:bg-orange-300" ></input>
                </div>
              </div>
        
              {korisnik.tip === "poslodavac" && (
                <div className="w-full">
                  <label htmlFor="adresa" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.adresa}</label>
                  <input
                    name="adresa"
                    type="text"
                    value={formData.adresa}
                    placeholder={jezik.formaProfil.adresa}
                    onChange={handleInputChange}
                    className="p-3 border rounded-lg w-full"
                  />
                </div>
              )}
       
              {korisnik.tip === "majstor" && (
                <div className="w-full">
                <label htmlFor="vestine" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.vestine}</label>
                <div className="flex flex-wrap justify-center pb-3">
                  <div className="flex flex-wrap">
                    <div>
                      <input
                        type="checkbox"
                        value="stolar"
                        className="vestine"
                        onChange={handleVestineChange}
                        checked={formData.vestine.includes("stolar")}
                      />
                      <label htmlFor="stolar" className="ps-3 pe-5">{jezik.formaProfil.stolar}</label>
                    </div>
                    <div>
                      <input
                        type="checkbox"
                        value="elektricar"
                        className="vestine"
                        onChange={handleVestineChange}
                        checked={formData.vestine.includes("elektricar")}
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
                        checked={formData.vestine.includes("vodoinstalater")}
                      />
                      <label htmlFor="vodoinstalater" className="ps-3 pe-5">{jezik.formaProfil.vodoinstalater}</label>
                    </div>
                    <div>
                      <input
                        type="checkbox"
                        value="keramicar"
                        className="vestine"
                        onChange={handleVestineChange}
                        checked={formData.vestine.includes("keramicar")}
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
        )}
        <button type="submit" className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">
          {jezik.general.submit}
        </button>
        </div>
      </form>
    </div>
    </div>
  );
}
