import * as React from 'react'

export interface DescriptionProps {
  text: string
  style?: React.CSSProperties
}

const Description = ({ text, style }: DescriptionProps): JSX.Element => {
  return (
        <p style={style}>{text}</p>
  )
}

export default Description
