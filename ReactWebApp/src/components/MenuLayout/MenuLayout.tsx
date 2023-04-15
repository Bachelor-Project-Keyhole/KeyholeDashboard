import * as React from 'react'

export interface MenuLayouotProps {
  style?: React.CSSProperties
  children?: React.ReactNode
}

const MenuLayout = ({ style, ...props }: MenuLayouotProps): JSX.Element => {
  document.body.style.margin = '0'
  return (
        <div style={{ ...wrapperStyles, ...style }}>
            <div style={{ ...innerStyles }}>
                <div style={{ marginRight: 10 }}>
                <div style={{ backgroundColor: 'rebeccapurple', height: '100px', width: '120px' }} />
                <div style={{ ...manageRowStyles }}>
                    <div style={{ backgroundColor: '#4caf50', height: '50px', width: '100%' }} />
                    <div style={{ backgroundColor: '#2196f3', height: '50px', width: '100%' }} />
                </div>
                </div>
                <div>
                <div style={{ backgroundColor: 'rebeccapurple', height: '100%', width: '120px' }} />
                </div>
                {props.children}
            </div>
        </div>
  )
}

// export interface WrapperStyles {

// }

export const wrapperStyles: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'column',
  justifyContent: 'center',
  alignItems: 'center',
  height: '100vh',
  backgroundColor: '#f1f1f1'
  // margin: '0',
  // padding: '50px',
}

export const innerStyles: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'row',
  // width: 50,
  // height: 50,
  backgroundColor: 'red',
  padding: '10px'
}

export const manageRowStyles: React.CSSProperties = {
  display: 'flex',
  flexDirection: 'row',
  justifyContent: 'space-between',
  alignItems: 'center',
  marginTop: 10,
  gap: 10
}

export default MenuLayout
