import { useContext, useEffect, useState } from "react"
import { useNavigate } from "react-router-dom"
import { AppContext } from "../App"

export default function FormaRecenzija(props){

    const [inputs, setInputs] = useState({opis: "", ocena: 0})
    const [images, setImages] = useState([])
    const navigate = useNavigate()
    const jezik = useContext(AppContext).jezik

    // useEffect(() => {
    //     document.getElementById('ocena').addEventListener("keypress", function (e) {
    //         var allowedChars = '12345'
    //         function contains(stringValue, charValue) {
    //             return stringValue.indexOf(charValue) > -1
    //         }
    //         var invalidKey = e.key.length === 1 && !contains(allowedChars, e.key)
    //                 || e.key === '.' && contains(e.target.value, '.')
    //         invalidKey && e.preventDefault()
    //     })
    // })

    const handleOpisChange = () => {
        const textOpis = document.getElementById("opis")
        setInputs({...inputs, ['opis']: textOpis.value})
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

    const handleSubmit = async e => {
        e.preventDefault()

        if (inputs.opis === '' || inputs.ocena === 0){
            window.alert(jezik.general.error.nepunaForma)
            return
        }

        const token = sessionStorage.getItem('jwt')
        const idUgovor = sessionStorage.getItem('uID')
        const idPrimalac = sessionStorage.getItem('kID')
        try {
            const response = await fetch('https://localhost:7080/Korisnik/NapraviRecenziju', {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `bearer ${token}`
                },
                body: JSON.stringify({
                    opis: inputs.opis,
                    ocena: inputs.ocena,
                    listaSlika: images,
                    idUgovor: idUgovor,
                    idPrimalac: idPrimalac
                })
            })
    
            if (response.ok){
                sessionStorage.removeItem('uID')
                sessionStorage.removeItem('kID')
                navigate('../profile')
            }
            else {
                window.alert(jezik.general.error.badRequest)
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
    }

    return (
        <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible">
            <div className="h-screen"></div>
            <div className="bg-white bg-opacity-70 shadow-lg p-8 w-full md:w-2/3 lg:w-1/2 xl:w-1/3 rounded-lg backdrop-blur-sm">  
                <form onSubmit={handleSubmit}>
                    <div className="flex flex-col space-y-4">
                        <div className="w-full">
                            <label htmlFor="ocena" className="block text-gray-700 font-bold mb-2 w-full">{jezik.pregledi.ocena}*</label>
                            <div className="w-full flex justify-center">
                                <div className="flex justify-evenly flex-nowrap bg-white border border-gray-200 rounded-lg w-3/5 min-w-max pb-2 px-2">
                                    <button type="button" onClick={() => setInputs({...inputs, ['ocena']: 1})} className={`text-5xl ${inputs.ocena >= 1 ? `text-yellow-500` : `text-gray-700`}`}>&#9733;</button>
                                    <button type="button" onClick={() => setInputs({...inputs, ['ocena']: 2})} className={`text-5xl ${inputs.ocena >= 2 ? `text-yellow-500` : `text-gray-700`}`}>&#9733;</button>
                                    <button type="button" onClick={() => setInputs({...inputs, ['ocena']: 3})} className={`text-5xl ${inputs.ocena >= 3 ? `text-yellow-500` : `text-gray-700`}`}>&#9733;</button>
                                    <button type="button" onClick={() => setInputs({...inputs, ['ocena']: 4})} className={`text-5xl ${inputs.ocena >= 4 ? `text-yellow-500` : `text-gray-700`}`}>&#9733;</button>
                                    <button type="button" onClick={() => setInputs({...inputs, ['ocena']: 5})} className={`text-5xl ${inputs.ocena >= 5 ? `text-yellow-500` : `text-gray-700`}`}>&#9733;</button>
                                </div>
                            </div>
                        </div>

                        <div className="w-full">
                            <label htmlFor="opis" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaRecenzija.opis}*</label>
                            <textarea
                                id="opis"
                                placeholder={jezik.formaRecenzija.opis}
                                onChange={handleOpisChange}
                                className={`p-3 border rounded-lg w-full ${inputs.opis ==='' ? "border-red-400" : "border-gray-100"}`}
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
                    
                        <button
                            type="submit"
                            className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300"  
                        >
                            {jezik.formaRecenzija.submit}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    )
}