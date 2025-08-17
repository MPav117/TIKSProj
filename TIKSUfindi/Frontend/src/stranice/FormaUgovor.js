import React, { useContext, useState } from 'react';
import { useLocation, useNavigate } from 'react-router-dom';
import { AppContext } from "../App";

export default function UgovorForma() {
    const location = useLocation();
    const navigate = useNavigate();
    const { zahtev, ugovorID, ugovor } = location.state || {}; 
    const { korisnik } = useContext(AppContext); // Pristup kontekstu
    const userType = korisnik ? korisnik.tip : null; // Dobijanje tipa korisnika iz konteksta
    const jezik = useContext(AppContext).jezik

    const [inputs, setInputs] = useState({
        ID: zahtev ? ugovorID : ugovor ? ugovor.id : '',
        ImeMajstora: '',
        ImePoslodavca: '',
        CenaPoSatu: zahtev ? zahtev.cenaPoSatu || '' : '',
        Opis: '',
        DatumPocetka: '',
        DatumZavrsetka: '',
        MajstorID: zahtev ? zahtev.majstorID : ugovor ? ugovor.majstorID : '',
        PoslodavacID: zahtev ? zahtev.poslodavacID : ugovor ? ugovor.poslodavacID : '',
        ZahtevZaPosaoID: zahtev ? zahtev.id : ugovor ? ugovor.zahtevPosaoID : '',
        potpisMajstora: '',
        potpisPoslodavca: '',
        slika: ''
    });

    //const [error, setError] = useState(null);

    const handleChange = (e) => {
        const { name, value } = e.target;
        setInputs((prevInputs) => ({ ...prevInputs, [name]: value }));
    };

    const handleSubmit = async (e) => {
        e.preventDefault();
        const token = sessionStorage.getItem("jwt");

        try {
            const response = await fetch('https://localhost:7080/Korisnik/potpisiUgovor', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `bearer ${token}`,
                },
                body: JSON.stringify({
                    ID: inputs.ID,
                    ImeMajstora: inputs.ImeMajstora,
                    ImePoslodavca: inputs.ImePoslodavca,
                    CenaPoSatu: parseFloat(inputs.CenaPoSatu),
                    Opis: inputs.Opis,
                    DatumPocetka: inputs.DatumPocetka,
                    DatumZavrsetka: inputs.DatumZavrsetka,
                    ZahtevZaPosaoID: inputs.ZahtevZaPosaoID,
                    MajstorID: inputs.MajstorID,
                    PoslodavacID: inputs.PoslodavacID,
                    potpisMajstora: inputs.potpisMajstora,
                    potpisPoslodavca: inputs.potpisPoslodavca,
                    slika: inputs.slika
                }),
            });

            if (!response.ok) {
                // const errorData = await response.json();
                // throw new Error(errorData.title || 'Network response was not ok');
                alert(jezik.general.error.badRequest);
            }
            else {
                //const data = await response.json();
                navigate('/profile');
            }
        } catch (error) {
            //setError(error.message);
            alert(jezik.general.error.netGreska + ": " + error.message);
        }
    };

    const handleImageChange = (e) => {
        const img = e.target.files[0];
        const reader = new FileReader();
        reader.onloadend = () => {
            if (userType === 'majstor') {
                setInputs((prevInputs) => ({ ...prevInputs, potpisMajstora: reader.result }));
            } else {
                setInputs((prevInputs) => ({ ...prevInputs, potpisPoslodavca: reader.result }));
            }
        };
        reader.readAsDataURL(img);
    };

    return (
        <div className="pozadina-forma-profil bg-fixed flex justify-center items-center bg-cover bg-center h-max overflow-visible">
            <div className="h-screen"></div>
            <div className="bg-white bg-opacity-70 shadow-lg p-8 w-full md:w-2/3 lg:w-1/2 xl:w-1/3 rounded-lg backdrop-blur-sm">  
                <form onSubmit={handleSubmit}>
                    <div className="flex flex-col space-y-4">
                        {userType !== 'poslodavac' && (
                            <div className="w-full">
                                <label htmlFor="ImeMajstora" className="block text-gray-700 font-bold mb-2">{jezik.formaUgovor.imem}*</label>
                                <input
                                    type="text"
                                    id="ImeMajstora"
                                    name="ImeMajstora"
                                    placeholder={jezik.formaUgovor.imem}
                                    value={inputs.ImeMajstora}
                                    onChange={handleChange}
                                    required
                                    className={`p-3 border rounded-lg w-full ${inputs.ImeMajstora ==='' ? "border-red-400" : "border-gray-100"}`}
                                />
                            </div>
                        )}
                        {userType !== 'majstor' && (
                            <div className="w-full">
                                <label htmlFor="ImePoslodavca" className="block text-gray-700 font-bold mb-2">{jezik.formaUgovor.imep}*</label>
                                <input
                                    type="text"
                                    id="ImePoslodavca"
                                    name="ImePoslodavca"
                                    placeholder={jezik.formaUgovor.imep}
                                    value={inputs.ImePoslodavca}
                                    onChange={handleChange}
                                    required
                                    className={`p-3 border rounded-lg w-full ${inputs.ImePoslodavca ==='' ? "border-red-400" : "border-gray-100"}`}
                                />
                            </div>
                        )}
                        <div className="w-full">
                            <label htmlFor="CenaPoSatu" className="block text-gray-700 font-bold mb-2">{jezik.pregledi.plata}*</label>
                            <input
                                type="number"
                                id="CenaPoSatu"
                                name="CenaPoSatu"
                                placeholder={jezik.pregledi.plata}
                                value={inputs.CenaPoSatu}
                                onChange={handleChange}
                                required
                                className={`p-3 border rounded-lg w-full ${inputs.CenaPoSatu ==='' ? "border-red-400" : "border-gray-100"}`}
                            />
                        </div>
                        <div className="w-full">
                            <label htmlFor="Opis" className="block text-gray-700 font-bold mb-2">{jezik.formaUgovor.opis}*</label>
                            <textarea
                                id="Opis"
                                name="Opis"
                                placeholder={jezik.formaUgovor.opis}
                                value={inputs.Opis}
                                onChange={handleChange}
                                required
                                className={`p-3 border rounded-lg w-full ${inputs.Opis ==='' ? "border-red-400" : "border-gray-100"}`}
                            />
                        </div>
                        <div className="w-full">
                            <label htmlFor="DatumPocetka" className="block text-gray-700 font-bold mb-2">{jezik.formaUgovor.datump}*</label>
                            <input
                                type="date"
                                id="DatumPocetka"
                                name="DatumPocetka"
                                value={inputs.DatumPocetka}
                                onChange={handleChange}
                                required
                                className={`p-3 border rounded-lg w-full ${inputs.DatumPocetka ==='' ? "border-red-400" : "border-gray-100"}`}
                            />
                        </div>
                        <div className="w-full">
                            <label htmlFor="DatumZavrsetka" className="block text-gray-700 font-bold mb-2">{jezik.formaUgovor.datumk}*</label>
                            <input
                                type="date"
                                id="DatumZavrsetka"
                                name="DatumZavrsetka"
                                value={inputs.DatumZavrsetka}
                                onChange={handleChange}
                                required
                                className={`p-3 border rounded-lg w-full ${inputs.DatumZavrsetka ==='' ? "border-red-400" : "border-gray-100"}`}
                            />
                        </div>
                        <div className="w-full">
                            <label htmlFor="pfp" className="block text-gray-700 font-bold mb-2">
                                {jezik.formaUgovor.potpis}*
                            </label>
                            <input
                                id="pfp"
                                type="file"
                                accept=".jpg, .jpeg, .png"
                                onChange={handleImageChange}
                                required
                                className={`p-3 border rounded-lg w-full ${inputs.potpisMajstora ==='' && inputs.potpisPoslodavca==='' ? "border-red-400" : "border-gray-100"}`}
                            />
                        </div>
                        {inputs.potpisMajstora && (
                            <div className="w-full flex justify-center">
                                <img src={inputs.potpisMajstora}alt={jezik.formaUgovor.potpis} className="h-40" />
                            </div>
                        )}
                        {inputs.potpisPoslodavca && (
                            <div className="w-full flex justify-center">
                                <img src={inputs.potpisPoslodavca} alt={jezik.formaUgovor.potpis} className="h-40" />
                            </div>
                        )}
                        <button
                            type="submit"
                            className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-4 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300"
                        >
                            {jezik.formaUgovor.potpisi}
                        </button>
                    </div>
                </form>
            </div>
        </div>
    );
}
