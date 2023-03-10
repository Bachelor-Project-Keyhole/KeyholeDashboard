import * as React from "react"

export interface TitleProps {
    text: string,
    style?: React.CSSProperties
}

const Title = ({text, style}: TitleProps) => {
    return (
        <h1 style={style}>{text}</h1>
    )
}

export default Title