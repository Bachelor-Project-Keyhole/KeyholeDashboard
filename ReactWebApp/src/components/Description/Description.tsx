import * as React from "react"

export interface TitleProps {
    text: string,
    style?: React.CSSProperties
}

const Description = ({text, style}: TitleProps) => {
    return (
        <p style={style}>{text}</p>
    )
}

export default Description