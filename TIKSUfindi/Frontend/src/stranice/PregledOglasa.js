import { useContext, useEffect, useState } from "react";
import ListaOglasa from "../komponente/ListaOglasa";
import { AppContext } from "../App";

export default function PregledOglasa(props){

    const [oglasi, setOglasi] = useState([])
    const [parametri, setParametri] = useState({sort: 'Ocena', minCenaPoSatu: -1, minOcenaPoslodavca: -1, idGrad: -1, reci: 'null'})
    const [isDropdownOpen, setDropdownOpen] = useState(false)
    const [vestine, setVestine] = useState([])
    const [gradovi, setGradovi] = useState([])
    const [stranicazaprgled, setStranicazaprgled] = useState(1);
    const [stranicazaprikaz, setStranicazaprikaz] = useState(1);
    const [kraj, setKraj] = useState(false);
    const jezik = useContext(AppContext).jezik

    const loadOglasi = async () => {
        const search = parametri.reci === '' ? 'null' : parametri.reci
        try {
            const response = await fetch(`https://localhost:7080/Osnovni/PregledOglasa/${parametri.sort}/${stranicazaprgled}?minCenaPoSatu=${parametri.minCenaPoSatu === "" ? -1 : parametri.minCenaPoSatu}&minOcenaPoslodavca=${parametri.minOcenaPoslodavca === "" ? -1 : parametri.minOcenaPoslodavca}&idGrad=${parametri.idGrad}&reci=${search}`, {
                method: "POST",
                headers: {'Content-Type': "application/json"},
                body: JSON.stringify(vestine)
            })
            if (response.ok){
                const data = await response.json()
                setOglasi(data.lista);
                setKraj(data.kraj);
                setStranicazaprikaz(stranicazaprgled);
            }
            else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
    }

    useEffect(() => {
        loadOglasi()
    }, [stranicazaprgled])

    const handleGradChange = async e => {
        if (e.target.value !== ""){
          try {
            const response = await fetch(`https://localhost:7080/Profil/GetGradovi?start=${e.target.value}`)
            if (response.ok){
                const data = await response.json()
                setGradovi(data)
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
        setParametri({...parametri, [e.target.name]: e.target.value})
    }

    const handleSubmit = e => {
        e.preventDefault()
        if(stranicazaprgled !== 1){
            setStranicazaprgled(1);
        } else {
            loadOglasi();
        }
    }

    const handleParamChange = e => {
        setParametri({...parametri, [e.target.name]: e.target.value})
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

    const handleNextPage = () => {
        if (!kraj) {
            setStranicazaprgled(stranicazaprgled + 1);
        }
    };

    const handlePreviousPage = () => {
        if (stranicazaprgled > 1) {
            setStranicazaprgled(stranicazaprgled - 1);
        }
    };

    return (
        <div className="flex flex-col items-center bg-[#ffedd5] min-h-screen p-4">  
            <div className="w-full sm:w-2/3 lg:w-1/2 pt-5">
                <form onSubmit={handleSubmit} className="flex flex-wrap items-center gap-4 w-full pb-3">
                    <input name='reci' type='text' placeholder={jezik.pregledi.search} onChange={handleParamChange} className="flex-grow min-w-0 flex-basis-0 px-3 py-2 border border-gray-300 rounded-md focus:outline-none focus:ring-2 focus:yellow-blue-500 focus:border-transparent"/>
                    <div className="flex flex-row gap-4 justify-end w-full sm:w-max">
                        <button type="submit" className="flex-shrink-0 bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.pregledi.search}</button>
                        <div className="relative flex-shrink-0">
                            <button onClick={() => setDropdownOpen(!isDropdownOpen)} className="bg-gray-400 bg-opacity-50 text-gray-800 font-semibold border border-gray-800 rounded-lg px-3 py-2 hover:bg-gray-400 hover:border-gray-700 hover:text-white transition-colors duration-300">{jezik.pregledi.filters}</button>

                            {isDropdownOpen && (
                                <div className="absolute right-0 top-10 p-4 bg-[#fafafa] border-2 border-zinc-200 rounded-lg divide-y-2 shadow-lg z-30 w-max">
                                    <div className="text-center grid grid-cols-2">
                                        <p className="py-2 col-span-2">{jezik.pregledi.filter}</p>
                                        <div className="flex flex-row py-2 col-span-2">
                                            <label className="w-1/2 text-right px-3 col-span-1">{jezik.pregledi.minPlata}: </label>
                                            <input id="cena" name="minCenaPoSatu" type="number" placeholder={jezik.pregledi.minPlata} onChange={handleParamChange} className="px-3 py-1 col-span-1"/>
                                        </div>
                                        <div className="flex-row py-2 col-span-2">
                                            <label className="w-1/2 text-right px-3 col-span-1">{jezik.pregledi.minPOcena}: </label>
                                            <input id="ocena" name="minOcenaPoslodavca" type="number" placeholder={jezik.pregledi.minPOcena} min={1} max={5} step={0.5} onChange={handleParamChange} className="px-3 py-1 col-span-1"/>
                                        </div>

                                        <div className="flex space-x-4 py-2 col-span-2 flex-wrap"> 
                                            <input
                                                type="checkbox"
                                                value="stolar"
                                                className="vestine"
                                                onChange={handleVestineChange}
                                            />
                                            <label htmlFor="stolar">{jezik.formaProfil.stolar}</label>
                                            <input
                                                type="checkbox"
                                                value="elektricar"
                                                className="vestine"
                                                onChange={handleVestineChange}
                                            />
                                            <label htmlFor="elektricar">{jezik.formaProfil.elektricar}</label>
                                            <input
                                                type="checkbox"
                                                value="vodoinstalater"
                                                className="vestine"
                                                onChange={handleVestineChange}
                                            />
                                            <label htmlFor="vodoinstalater">{jezik.formaProfil.vodoinstalater}</label>
                                            <input
                                                type="checkbox"
                                                value="keramicar"
                                                className="vestine"
                                                onChange={handleVestineChange}
                                            />
                                            <label htmlFor="keramicar">{jezik.formaProfil.keramicar}</label>
                                        </div>
                                        
                                        <div className="py-2 col-span-2">
                                            <input
                                                name="drugo"
                                                type="text"
                                                placeholder={jezik.formaProfil.drugo}
                                                id="drugo"
                                                onChange={handleVestineChange}
                                                className="px-3 py-1 border rounded text-start justify-self-start"
                                            />
                                        </div>

                                        <div className="flex flex-row py-2 col-span-2">
                                            <input type="text" onChange={handleGradChange} placeholder={jezik.formaProfil.grad} className="w-1/2 px-3 py-1 col-span-1"></input>
                                            <select name="idGrad" onChange={handleGradSelect} className="w-1/2 px-3 py-1 col-span-1">
                                                <option value={-1}>None</option>
                                                {gradovi.map(grad => (
                                                    <option key={grad.id} value={grad.id}>{grad.city_ascii}, {grad.country}</option>
                                                ))}
                                            </select>
                                        </div>
                                    </div>
                                    <div className="text-center">
                                        <p className="py-2 col-span-2">{jezik.pregledi.sort}</p>
                                        <div className="py-2 col-span-2">
                                            <input type="radio" name="sort" value={'Ocena'} onClick={handleParamChange} defaultChecked></input><label className="pe-6">{jezik.pregledi.ocena}</label>
                                            <input type="radio" name="sort" value={'Cena po satu'} onClick={handleParamChange}></input><label>{jezik.pregledi.plata}</label>
                                        </div>
                                    </div>
                                </div>    
                            )}
                        </div>
                    </div>
                </form>
                {(oglasi !== null && oglasi.length > 0) && (
                <ListaOglasa lista={oglasi}></ListaOglasa>
            )}
            <div className="flex justify-between mt-4">
                <button
                    onClick={handlePreviousPage}
                    className="bg-gray-400 bg-opacity-50 text-gray-800 font-semibold border border-gray-800 rounded-lg px-3 py-2 hover:bg-gray-400 hover:border-gray-700 hover:text-white transition-colors duration-300"
                    disabled={stranicazaprgled === 1}
                    >
                    {jezik.pregledi.prev}
                </button>
                <span>{jezik.pregledi.page} {stranicazaprikaz}</span>
                <button
                    onClick={handleNextPage}
                    className="bg-gray-400 bg-opacity-50 text-gray-800 font-semibold border border-gray-800 rounded-lg px-3 py-2 hover:bg-gray-400 hover:border-gray-700 hover:text-white transition-colors duration-300"
                    disabled={kraj}
                >
                    {jezik.pregledi.next}
                </button>
            </div>
        </div>
    </div>
  )
}