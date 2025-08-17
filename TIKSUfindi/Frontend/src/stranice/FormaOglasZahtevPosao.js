import { useContext, useEffect, useState } from "react";
import { useNavigate } from "react-router-dom";
import { AppContext } from "../App";

export default function FormaOglasZahtevPosao(props){
    const oglas = useContext(AppContext).oglas
    const [inputs, setInputs] = useState({naslov: '', opis: "", datumZavrsetka: "", cenaPoSatu: ""})
    const [curDate, setCurDate] = useState(new Date().toISOString().slice(0, 10))
    const [date, setDate] = useState(props.stanje === 'oglas' || (props.stanje === 'zahtev' && oglas === null) ? curDate : new Date(oglas.datumZavrsetka))
    const [images, setImages] = useState([])
    const [vestine, setVestine] = useState([])
    const setOglas = useContext(AppContext).setOglas
    const jezik = useContext(AppContext).jezik

    const navigate = useNavigate()

    useEffect(() => {
        document.getElementById('cenaPoSatu').addEventListener("keypress", function (e) {
            var allowedChars = '0123456789.'
            function contains(stringValue, charValue) {
                return stringValue.indexOf(charValue) > -1
            }
            var invalidKey = e.key.length === 1 && !contains(allowedChars, e.key)
                    || e.key === '.' && contains(e.target.value, '.')
            invalidKey && e.preventDefault()
        })
    })

    useEffect(() => {
        const oglasId = sessionStorage.getItem('oglas')
        if (props.stanje === 'zahtev' && oglasId !== null && oglasId !== ''){
            loadOglas()
        }
    }, [])

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

    const loadOglas = async () => {
        const oglasId = sessionStorage.getItem('oglas')
        try {
            const response = await fetch(`https://localhost:7080/Osnovni/GetOglas/${oglasId}`)
            if (response.ok){
                const data = await response.json()
                setOglas(data)
                setInputs({
                    opis: oglas.opis,
                    datumZavrsetka: oglas.datumZavrsetka,
                    cenaPoSatu: oglas.cenaPoSatu
                })
            }
            else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
    }

    const handleSubmit = async e => {
        e.preventDefault()

        if (props.stanje === 'oglas'){
            if (inputs.naslov === '' || inputs.opis === "" || inputs.datumZavrsetka === "" || inputs.cenaPoSatu === "" || vestine === null){
                window.alert(jezik.general.error.nepunaForma)
                return
            }
        }
        else if (props.stanje === 'zahtev'){
            if (inputs.opis === "" || inputs.datumZavrsetka === "" || inputs.cenaPoSatu === ""){
                window.alert(jezik.general.error.nepunaForma)
                return
            }
        }

        const token = sessionStorage.getItem('jwt')
        let response = null

        try {
            if (props.stanje === 'oglas'){
                response = await fetch('https://localhost:7080/Poslodavac/postaviOglas', {
                    method: "POST",
                    headers: {
                        'Content-Type': "application/json",
                        Authorization: `bearer ${token}`
                    },
                    credentials: 'include',
                    body: JSON.stringify({
                        naslov: inputs.naslov,
                        opis: inputs.opis,
                        cenaPoSatu: inputs.cenaPoSatu,
                        listaSlika: images,
                        datumZavrsetka: inputs.datumZavrsetka,
                        listaVestina: vestine
                    })
                })
            }
            else{
                const viewid = sessionStorage.getItem('view')
                response = await fetch('https://localhost:7080/Poslodavac/napraviZahtevPosao', {
                    method: "POST",
                    headers: {
                        'Content-Type': "application/json",
                        Authorization: `bearer ${token}`
                    },
                    credentials: 'include',
                    body: JSON.stringify({
                        opis: inputs.opis,
                        cenaPoSatu: inputs.cenaPoSatu,
                        listaSlika: images,
                        datumZavrsetka: inputs.datumZavrsetka,
                        korisnikID: viewid,
                        oglasID: oglas === null ? -1 : oglas.id
                    })
                })
            }
            
    
            if (response.ok){
                navigate('../profile')
            }
            else {
                window.alert(jezik.general.error.badRequest)
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
    }

    const handleInputChange = e => {
        setInputs({...inputs, [e.target.name]: e.target.value})
    }

    const handleOpisChange = () => {
        const textOpis = document.getElementById("opis")
        setInputs({...inputs, ["opis"]: textOpis.value})
    }

    const handleDateChange = () => {
        const dateval = new Date(document.getElementById('date').value)
        setInputs({...inputs, ['datumZavrsetka']: dateval.toJSON()})
        setDate(dateval.toISOString().slice(0, 10))
    }

    const handleImageChange = async e => {
        const imgs = [...e.target.files]
        let newImgs = await Promise.all(imgs.map(img => {
            return processImg(img)
        }))
        setImages(newImgs)
    }

    const processImg = (image) => {
        return new Promise((resolve, reject) => {
			let fileReader = new FileReader()
			fileReader.onload = () => {
				return resolve(fileReader.result)
			}
			fileReader.readAsDataURL(image)
		})
    }

    return (
        <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible">
            <div className="h-screen"></div>
            <div className="bg-white bg-opacity-70 shadow-lg p-8 w-full md:w-2/3 lg:w-1/2 xl:w-1/3 rounded-lg backdrop-blur-sm">  
                <form onSubmit={handleSubmit}>
                    <div className="flex flex-col space-y-4">
                        {props.stanje === 'oglas' && (
                            <>
                                <div className="w-full">
                                    <label htmlFor="naslov" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.naslov}*</label>
                                    <input
                                        name="naslov"
                                        type="text"
                                        placeholder={jezik.formaProfil.naslov}
                                        onChange={handleInputChange}
                                        className={`p-3 border rounded-lg w-full ${inputs.naslov ==='' ? "border-red-400" : "border-gray-100"}`}
                                        required
                                    />
                                </div>
        
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
                            </>
                        )}

                        <div className="w-full">
                            <label htmlFor="opis" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaProfil.opis}*</label>
                            <textarea
                                id="opis"
                                value={inputs.opis}
                                placeholder={jezik.formaProfil.opis}
                                onChange={handleOpisChange}
                                className={`p-3 border rounded-lg w-full ${inputs.opis ==='' ? "border-red-400" : "border-gray-100"}`}
                                required
                            />
                        </div>

                        <div className="w-full">
                            <label htmlFor="cenaPoSatu" className="block text-gray-700 font-bold mb-2 w-full">{jezik.pregledi.plata}*</label>
                            <input
                                id="cenaPoSatu"
                                name="cenaPoSatu"
                                type="number"
                                value={inputs.cenaPoSatu}
                                step='0.01'
                                min={0}
                                placeholder={jezik.pregledi.plata}
                                onChange={handleInputChange}
                                className={`p-3 border rounded-lg w-full ${inputs.cenaPoSatu ==='' ? "border-red-400" : "border-gray-100"}`}
                                required
                            />
                        </div>

                        <div className="w-full">
                            <label htmlFor="date" className="block text-gray-700 font-bold mb-2 w-full">{jezik.oglas.datumDo}*</label>
                            <input
                                id="date"
                                type="date"
                                min={curDate}
                                onChange={handleDateChange}
                                className={`p-3 border rounded-lg w-full ${inputs.datumZavrsetka ==='' ? "border-red-400" : "border-gray-100"}`}
                                required
                            />
                        </div>

                        <div className={`w-full flex-row relative ${images !== null && images.length > 0 && 'h-64'} align-middle`}>
                            <label className="block text-gray-700 font-bold mb-2 w-full">{jezik.oglas.slike}</label>
                            <input
                              type="file"
                              accept=".jpg, .jpeg, .png"
                              multiple
                              onChange={handleImageChange}
                              className="p-3 border rounded-lg"
                            />
                            {images !== null && (
                                <div className="flex flex-row space-x-4 flex-nowrap overflow-auto justify-center pt-3 w-full">
                                    {images.map((img, index) => (
                                        <img key={index} src={img} alt={jezik.formaProfil.slika} className="h-40 object-cover" />
                                    ))}
                                </div>
                            )}
                        </div>



                        <button type="submit" className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">
                            {jezik.general.submit}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    )
}