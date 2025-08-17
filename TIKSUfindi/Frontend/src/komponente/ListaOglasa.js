import { memo } from "react";
import Oglas from "./Oglas"

function ListaOglasa(props){
    return props.lista.map((oglas, index) => (
        <Oglas key={index} oglas={oglas}></Oglas>
    ))
}

export default memo(ListaOglasa)