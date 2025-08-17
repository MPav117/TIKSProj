import { useContext, useState } from "react"
import { useNavigate } from "react-router-dom"
import { AppContext } from "../App"

export default function FormaZahtevGrupa(props){

    const [inputs, setInputs] = useState('')
    const navigate = useNavigate()
    const jezik = useContext(AppContext).jezik

    const handleOpisChange = () => {
        const textOpis = document.getElementById("opis")
        setInputs(textOpis.value)
    }

    const handleSubmit = async (e) => {
        e.preventDefault()

        if (inputs === ''){
            window.alert(jezik.general.error.nepunaForma)
            return
        }

        const token = sessionStorage.getItem('jwt')
        const primalac = sessionStorage.getItem('view')
        const url = `https://localhost:7080/Majstor/napraviZahtevGrupa/` + primalac

        try {
            const response = await fetch(url, {
                method: 'POST',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `bearer ${token}`
                },
                body: JSON.stringify(inputs)
            })
    
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

    return (
        <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible">
            <div className="h-screen"></div>
            <div className="bg-white bg-opacity-70 shadow-lg p-8 w-full md:w-2/3 lg:w-1/2 xl:w-1/3 rounded-lg backdrop-blur-sm">  
                <form onSubmit={handleSubmit}>
                    <div className="flex flex-col space-y-4">
                        <div className="w-full">
                            <label htmlFor="opis" className="block text-gray-700 font-bold mb-2 w-full">{jezik.formaGrupa.opis}*</label>
                            <textarea
                                id="opis"
                                placeholder={jezik.formaGrupa.opis}
                                onChange={handleOpisChange}
                                className={`p-3 border rounded-lg w-full ${inputs ==='' ? "border-red-400" : "border-gray-100"}`}
                                required
                            />
                        </div>
                    
                        <button
                            type="submit"
                            className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300"  
                        >
                            {jezik.formaGrupa.submit}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    )
}