import { isBefore, isSameDay, isWithinInterval } from "date-fns";
import { memo, useContext, useEffect, useState } from "react";
import Calendar from "react-calendar";
import { AppContext } from "../App";
import '../calendar_styles.css';

//https://www.npmjs.com/package/react-calendar
//npm install react-calendar
//npm i date-fns

function Kalendar(props) {
    const [ugovorDani, setUgovorDani] = useState([]);
    const [clickState, setClickState] = useState('sDay');
    const [odmorDaniList, setOdmorDaniList] = useState([]);
    const [flag, setFlag] = useState(false)
    const [lastOdmor, setLastOdmor] = useState(new Date())
    const jezik = useContext(AppContext).jezik

    const korisnik = useContext(AppContext).korisnik

    useEffect(() => {
        loadKalendar()
    }, []);

    const loadKalendar = async () => {
        let response = null
        if (props.licniProfil){
            try {
                response = await fetch(`https://localhost:7080/Majstor/getKalendar/${korisnik.id}`)
            } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
            }
        }
        else {
            const viewId = sessionStorage.getItem('view')
            try {
                response = await fetch(`https://localhost:7080/Majstor/getKalendar/${viewId}`)
            } catch (error) {
            window.alert(jezik.general.error.netGreska + ": " + error.message)
            }
        }

        if (response.ok){
            const data = await response.json()

            let noviUgovorDani = [];
            data.listaPocetnihDatumaUgovora.forEach((date, index) => {
                noviUgovorDani.push([new Date(date), new Date(data.listaKrajnjihDatumaUgovora[index])]);
            });
            setUgovorDani(noviUgovorDani);

            let noviOdmorDani = odmorDaniList === null ? [] : odmorDaniList;
            let newLastDay = new Date()
            data.pocetniDatumi.forEach((date, index) => {
                const limit = new Date(data.krajnjiDatumi[index])
                for (let d = new Date(date); d <= limit; d.setDate(d.getDate() + 1)) {
                    if (!noviOdmorDani.find(dDate => isSameDay(dDate, d))) {
                        noviOdmorDani.push(new Date(d));
                        if (isBefore(newLastDay, d) && !isSameDay(newLastDay, d)){
                            newLastDay = new Date(d)
                        }
                    }
                }
            });
            setOdmorDaniList(noviOdmorDani)
            if (isBefore(lastOdmor, newLastDay) && !isSameDay(lastOdmor, newLastDay)){
                setLastOdmor(new Date(newLastDay))
            }
        }
        else {
            window.alert(jezik.general.error.netGreska + ": " + await response.text())
        }
    }

    function tileDisabled({ date, view }) {
        if (view === 'month') {
            return isWithinRanges(date, ugovorDani);
        }
    }

    function isWithinRange(date, range) {
        return isWithinInterval(date, { start: range[0], end: range[1] });
    }

    function isWithinRanges(date, ranges) {
        return ranges.some(range => isWithinRange(date, range));
    }

    function tileClassName({ date, view }) {
        if (view === 'month') {
            if (isWithinRanges(date, ugovorDani) || (isBefore(date, new Date()) && !isSameDay(date, new Date()))) {
                return ['bg-gray-400', 'text-gray-500'];
            } else if (odmorDaniList.find(dDate => isSameDay(dDate, date))) {
                return props.licniProfil ? 'bg-gray-300' : ['bg-gray-400', 'text-gray-500'] //ovo drugo obavezno isto kao ovo gore
            }
        }
    }
    
    const handleClick = (value) => {
        switch (clickState) {
            case 'sDay':
                handleSingleDay(value);
                break; 
            case 'fDay':
                handleFreeDay(value);
                break;
            case 'sRange':
                handleRangeSelection(value);
                break;
            case 'fRange':
                handleRangeRemoval(value);
                break;
            default:
                break;
        }
    };
    
    const handleSingleDay = (value) => {
        if (!odmorDaniList.find(dDate => isSameDay(dDate, value))) {
            setOdmorDaniList([...odmorDaniList, new Date(value)]);
        }
        if (isBefore(lastOdmor, value) && !isSameDay(lastOdmor, value)){
            setLastOdmor(new Date(value))
        }
    };
    
    const handleFreeDay = (value) => {
        setOdmorDaniList(odmorDaniList.filter(dDate => !isSameDay(dDate, value)));

        if (isSameDay(value, lastOdmor)){
            let limit = new Date()
            let d = value
            for (d.setDate(d.getDate() - 1); d >= limit; d.setDate(d.getDate() - 1)) {
                if (odmorDaniList.find(dDate => isSameDay(dDate, d))){
                    setLastOdmor(new Date(d))
                    return
                }
            }
            setLastOdmor(limit)
        }
    };
    
    const handleRangeSelection = (value) => {
        if (!flag){
            setFlag(true)
            return
        }
        
        let newOdmorDani = [...odmorDaniList];
        for (let d = value[0]; d <= value[1]; d.setDate(d.getDate() + 1)) {
            if (!newOdmorDani.find(dDate => isSameDay(dDate, d))) {
                newOdmorDani.push(new Date(d));
            }
        }

        if (isBefore(lastOdmor, value[1]) && !isSameDay(lastOdmor, value[1])){
            setLastOdmor(new Date(value[1]))
        }

        setOdmorDaniList(newOdmorDani);
    };

    const handleRangeRemoval = (value) => {
        if (!flag){
            setFlag(true)
            return
        }
        
        let newOdmorDani = [...odmorDaniList];
        for (let d = new Date(value[0]); d <= value[1]; d.setDate(d.getDate() + 1)) {
            newOdmorDani = newOdmorDani.filter(dDate => !isSameDay(new Date(d), dDate))
        }
        setOdmorDaniList(newOdmorDani);
        
        if (isWithinRange(lastOdmor, value)){
            let limit = new Date()
            let d = new Date(value[0])
            for (d.setDate(d.getDate() - 1); d >= limit; d.setDate(d.getDate() - 1)) {
                if (newOdmorDani.find(dDate => isSameDay(dDate, d))){
                    setLastOdmor(new Date(d))
                    return
                }
            }
            setLastOdmor(limit)
        }
    };

    const handleSubmit = async () => {
        let newOdmorStarts = []
        let newOdmorEnds = []
        let prevDay = null
        let inRange = false
        for (let d = new Date(); isBefore(d, lastOdmor) || isSameDay(d, lastOdmor); d.setDate(d.getDate() + 1)) {
            if (!inRange && odmorDaniList.find(dDate => isSameDay(dDate, d))){
                inRange = true
                newOdmorStarts.push(new Date(d))
            }
            else if (inRange && !odmorDaniList.find(dDate => isSameDay(dDate, d))){
                inRange = false
                newOdmorEnds.push(new Date(prevDay))
            }

            if (isSameDay(d, lastOdmor)) {
                newOdmorEnds.push(new Date(d))
            }

            prevDay = new Date(d)
        }

        const token = sessionStorage.getItem('jwt')
        try {
            const response = await fetch('https://localhost:7080/Majstor/UpisiKalendar', {
                method: 'PUT',
                headers: {
                    'Content-Type': 'application/json',
                    Authorization: `bearer ${token}`,
                },
                body: JSON.stringify({
                    pocetniDatumi: newOdmorStarts,
                    krajnjiDatumi: newOdmorEnds
                })
            })
    
            if (!response.ok){
                window.alert(jezik.general.error.netGreska + ": " + await response.text())
            }
        } catch (error) {
        window.alert(jezik.general.error.netGreska + ": " + error.message)
        }
    }

    return (
        <>
            <Calendar className="calendar-container" onChange={props.licniProfil ? handleClick : () => {}} selectRange={flag && (clickState === 'sRange' || clickState === 'fRange')}  minDetail="year" minDate={new Date()} prev2Label={null} next2Label={null} tileDisabled={tileDisabled} tileClassName={tileClassName}/>
            {props.licniProfil && (
                <>
                    <div className="flex justify-center space-x-4 mb-4 pt-7">
                        <button
                            className={`px-4 py-2 rounded border ${clickState === 'sDay' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                            onClick={() => {
                                setClickState('sDay')
                                setFlag(false)
                            }}
                            >
                            {jezik.kalendar.sd}
                        </button>
                        <button
                            className={`px-4 py-2 rounded border ${clickState === 'sRange' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                            onClick={() => {
                                setClickState('sRange')
                            }}
                            >
                            {jezik.kalendar.sr}
                        </button>
                        <button
                            className={`px-4 py-2 rounded border ${clickState === 'fDay' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                            onClick={() => {
                                setClickState('fDay')
                                setFlag(false)
                            }}
                            >
                            {jezik.kalendar.fd}
                        </button>
                        <button
                            className={`px-4 py-2 rounded border ${clickState === 'fRange' ? 'bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg px-4 py-2' : 'border-yellow-600 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300'}`}
                            onClick={() => {
                                setClickState('fRange')
                            }}
                            >
                            {jezik.kalendar.fr}
                        </button>
                    </div>
                    <div className="flex justify-center space-x-4 mb-4 pt-3">
                        <button onClick={handleSubmit} className="px-4 bg-yellow-700 bg-opacity-50 text-yellow-800 font-semibold border border-yellow-800 rounded-lg py-2 hover:bg-yellow-600 hover:border-yellow-700 hover:text-white transition-colors duration-300">{jezik.kalendar.save}</button>
                    </div>
                </>
            )}
        </>
    );
}

export default memo(Kalendar);

