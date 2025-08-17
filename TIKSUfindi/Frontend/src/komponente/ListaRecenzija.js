import { memo, useContext, useEffect, useState } from "react";
import { AppContext } from "../App";
import Loader from "./Loader";

function ListaRecenzija(props){
    const [recenzije, setRecenzije] = useState(props.lista);
    const jezik = useContext(AppContext).jezik;
    const korisnik = useContext(AppContext).korisnik;
    const [currentImageIndexes, setCurrentImageIndexes] = useState({});
    const [isUgovorVisible, setIsUgovorVisible] = useState(false);
    const [visibleContractIndex, setVisibleContractIndex] = useState(null);


    useEffect(() => {
        if (recenzije === null){
            loadRecenzije();
        } else {
            // Initialize the currentImageIndexes state with zeros for each review
            const initialIndexes = recenzije.reduce((acc, _, index) => ({ ...acc, [index]: 0 }), {});
            setCurrentImageIndexes(initialIndexes);
        }
    }, [recenzije]);

    const loadRecenzije = async () => {
        const viewId = sessionStorage.getItem('view');
        try {
            const response = await fetch(`https://localhost:7080/Osnovni/GetRecenzije/${viewId}`);
            if (response.ok){
                const data = await response.json();
                setRecenzije(data);
            } else {
                window.alert(jezik.general.error.netGreska + ": " + await response.text());
            }
        } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message);
        }
    };

    if (recenzije === null){
        return <Loader />
    }

    const handlePreviousImage = (index) => {
        setCurrentImageIndexes((prevIndexes) => ({
            ...prevIndexes,
            [index]: prevIndexes[index] === 0 ? recenzije[index].listaSlika.length - 1 : prevIndexes[index] - 1
        }));
    };

    const handleNextImage = (index) => {
        setCurrentImageIndexes((prevIndexes) => ({
            ...prevIndexes,
            [index]: prevIndexes[index] === recenzije[index].listaSlika.length - 1 ? 0 : prevIndexes[index] + 1
        }));
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
  const formatDate = (dateString) => {
    const date = new Date(dateString);
    const month = date.getMonth() + 1; 
    const day = date.getDate();
    const year = date.getFullYear();
    return `${month}/${day}/${year}`;
  };

  const toggleContractVisibility = (index) => {
    setVisibleContractIndex(visibleContractIndex === index ? null : index);
};

return (
    <div className="w-full max-w-4xl bg-white bg-opacity-70 border border-gray-200 rounded-lg shadow mx-auto mt-4 container">
        <h2 className='text-center text-2xl font-semibold pt-5'>{recenzije === null || recenzije.length === 0 ? jezik.recenzije.nema : jezik.stranicaProfil.recenzije}</h2>
        <div className="flex flex-row flex-wrap justify-center gap-4 p-4">
            {recenzije.map((recenzija, index) => (
                <div key={index} className="rounded bg-orange-100 border border-orange-200 shadow-sm relative w-max max-w-1/2">
                    <div className="p-6">
                        <div className="relative mb-4">
                            <div className="absolute h-20 w-20 -right-4 -top-12">
                                <img
                                    src={recenzija.slikaPoslodavca}
                                    alt="Slika poslodavca"
                                    className="w-full h-full object-cover rounded-full"
                                />
                            </div>
                        </div>
                        <h3 className="text-2xl font-semibold mb-4 text-center">{recenzija.imeDavalac}</h3>
                        <div className="flex items-center justify-center mb-4">
                            <span className="text-gray-800 font-bold text-xl mr-2">{jezik.pregledi.ocena}:</span>
                            {renderStars(recenzija.ocena)}
                        </div>
                        <p className="text-lg text-gray-700 mb-4 text-center">{recenzija.opis}</p>
                        <div className="mb-4">
                            <div className="flex justify-center items-center">
                                <button className="bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-3 py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300" onClick={() => toggleContractVisibility(index)}>
                                    {jezik.recenzije.ugovor}
                                </button>
                            </div>
                            {visibleContractIndex === index && (
                                <div className="mt-4">
                                    <p className="text-lg text-gray-700 mb-4 text-center">{jezik.profilUgovori.dpocetak}: {formatDate(recenzija.ugovor.datumPocetka)} Datum zavrsetka: {formatDate(recenzija.ugovor.datumZavrsetka)}.</p>
                                    <p className="text-lg text-gray-700 mb-4 text-center">{jezik.pregledi.plata}: {recenzija.ugovor.cenaPoSatu} EUR</p>
                                    <p className="text-lg text-gray-700 mb-4 text-center text-wrap">{recenzija.ugovor.opis}</p>
                                </div>
                            )}
                        </div>
                        {recenzija.listaSlika && recenzija.listaSlika.length > 0 && (
                            <div className="flex flex-col items-center relative">
                                <div className="relative">
                                    <img
                                        className="w-auto max-w-full h-80 p-2 shadow-lg"
                                        src={recenzija.listaSlika[currentImageIndexes[index]] || "/images/"}
                                        alt="slike"
                                    />
                                    {recenzija.listaSlika.length > 1 && (
                                        <>
                                            <button
                                                className="absolute top-1/2 left-4 transform -translate-y-1/2 bg-gray-200 rounded-full p-2 opacity-50"
                                                onClick={() => handlePreviousImage(index)}
                                            >
                                                &lt;
                                            </button>
                                            <button
                                                className="absolute top-1/2 right-4 transform -translate-y-1/2 bg-gray-200 rounded-full p-2 opacity-50"
                                                onClick={() => handleNextImage(index)}
                                            >
                                                &gt;
                                            </button>
                                        </>
                                    )}
                                </div>
                            </div>
                        )}
                    </div>
                </div>
            ))}
        </div>
    </div>
);
}

export default memo(ListaRecenzija);
