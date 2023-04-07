import * as React from 'react'

export interface HeaderProps {
  text: string
  style?: React.CSSProperties
}

const Header = ({ text, style }: HeaderProps) => {
  return (
        <h2 style={style}>{text}</h2>
  )
}

export default Header
