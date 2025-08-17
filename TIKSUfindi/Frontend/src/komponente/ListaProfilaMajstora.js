import { memo, useContext } from "react";
import ProfilMajstora from "./ProfilMajstora";
import { AppContext } from "../App";
import Loader from "./Loader";

function ListaProfilaMajstora(props){
    const jezik = useContext(AppContext).jezik

    if (props.lista === null){
        return <Loader />
    }

    return props.lista.map((profil, index) => (
        <ProfilMajstora key={index} majstor={profil} idPoslodavca={props.idPoslodavca} oglas={props.oglas} count={props.lista.length}></ProfilMajstora>
    ))
}

export default memo(ListaProfilaMajstora)