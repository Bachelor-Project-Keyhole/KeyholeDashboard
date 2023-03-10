import * as React from "react"

export interface ButtonProps{
    text: string,
    style?: React.CSSProperties,
    onClick?: () => void
    solid?: boolean
    icon?: React.ReactNode
}

const Button = ({icon, text, solid, style, onClick}: ButtonProps) => {
    return (
        <button style={{...buttonStyles, ...style}} onClick={onClick}>
            {icon}
            {text}
        </button>
    )
}

export interface ButtonStyles {
    icon: React.CSSProperties,
    text: React.CSSProperties,
}

export const buttonStyles : React.CSSProperties = {
    height: 50,
    width: 100,
        
}

        
    


    

export default Button