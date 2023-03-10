import * as React from "react"

export interface TitleProps {
    text: string,
    style?: React.CSSProperties
}

const Header = ({text, style}: TitleProps) => {
    return (
        <h2 style={style}>{text}</h2>
    )
}

export default Header