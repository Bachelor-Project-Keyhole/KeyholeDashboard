import * as React from "react"

export interface TitleProps {
    text: string,
    style?: React.CSSProperties
}

const SubHeader = ({text, style}: TitleProps) => {
    return (
        <h3 style={style}>{text}</h3>
    )
}

export default SubHeader