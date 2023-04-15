import * as React from 'react'

export interface SubHeaderProps {
  text: string
  style?: React.CSSProperties
}

const SubHeader = ({ text, style }: SubHeaderProps): JSX.Element => {
  return (
        <h3 style={style}>{text}</h3>
  )
}

export default SubHeader
