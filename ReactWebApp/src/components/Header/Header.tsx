import * as React from 'react'

export interface HeaderProps {
  text: string
  style?: React.CSSProperties
}

const Header = ({ text, style }: HeaderProps): JSX.Element => {
  return (
        <h2 style={style}>{text}</h2>
  )
}

export default Header
