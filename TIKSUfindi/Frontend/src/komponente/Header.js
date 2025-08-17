import { useContext, useState } from "react";
import { Link } from "react-router-dom";
import { AppContext } from "../App";

export default function Header() {
  const [isDropdownOpen, setDropdownOpen] = useState(false)
  const [jezikDDown, setJezikDDOwn] = useState(false)
  const korisnik = useContext(AppContext).korisnik
  const setNaProfilu = useContext(AppContext).setNaProfilu
  const jezik = useContext(AppContext).jezik
  const jezikID = useContext(AppContext).jezikID
  const setJezikID = useContext(AppContext).setJezikID
  const [jezici, setJezici] = useState([{id: 'en', name: 'English'}, {id: 'sr', name: 'Srpski'}])

  const toggleDropdown = () => {
    setDropdownOpen(!isDropdownOpen);  //za promenu stanja dropdown-a
  };

  const toggleJezikDDown = () => {
    setJezikDDOwn(!jezikDDown)
  }

  const handleJezikChange = id => {
    setJezikDDOwn(false)
    setJezikID(id)
    sessionStorage.setItem('jezik', id)
  }

  return (
    <div className="flex justify-between items-center h-14 bg-gradient-to-r from-amber-600 to-amber-800 relative z-20">  {/* Glavni navbar s pozadinskom bojom */}
      <Link to="/" className="font-bold text-white hover:text-zinc-700 logo ps-8 pe-4 h-10 w-36 bg-origin-content bg-contain bg-no-repeat" />

      <div className="flex space-x-4">
        <div className="relative">
          <button onClick={toggleJezikDDown} className="text-white hover:text-zinc-700 pe-5">{jezikID.toUpperCase()}</button>
          {jezikDDown && (
            <div
              onClick={() => setDropdownOpen(false)}
              className="absolute right-0 mt-2 w-max bg-yellow-700 bg-opacity-50 rounded-lg shadow-lg z-30"
            >
              {jezici.map(jezik => (
                <button key={jezik.id} onClick={() => handleJezikChange(jezik.id)} className="block px-4 py-2 text-white hover:bg-yellow-600 rounded w-full">{jezik.name}</button>
              ))}
            </div>
          )}

        </div>
        {korisnik === null && (
          <>
          <Link to="/login" onClick={() => setDropdownOpen(false)} className="text-white hover:text-zinc-700">{jezik.header.login}</Link>
          <div className="relative">
            <button onClick={toggleDropdown} className="text-white hover:text-zinc-700 pe-5">{jezik.header.register}</button>
            
            {isDropdownOpen && (
              <div
                onClick={() => setDropdownOpen(false)}
                className="absolute right-0 mt-2 w-40 bg-yellow-700 bg-opacity-50 rounded-lg shadow-lg z-30"
              >
                <Link
                  to="/register_craftsman"
                  className="block px-4 py-2 text-white hover:bg-yellow-600 rounded"
                >{jezik.header.majstor}</Link>

                <Link
                  to="/register_employer"
                  className="block px-4 py-2 text-white hover:bg-yellow-600 rounded"
                >{jezik.header.poslodavac}</Link>
              </div>
            )}

          </div>
          </>
          
        )}
        {korisnik !== null && (
          <Link to="/profile" onClick={() => {setNaProfilu(true)}} className="text-white hover:text-zinc-700 pe-5">{jezik.header.profil}</Link>
        )}
      </div>
    </div>
  );
}
