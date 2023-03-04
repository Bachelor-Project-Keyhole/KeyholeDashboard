import { get, merge } from 'lodash'
import * as React from 'react'
import axios from 'axios'


export interface IWeatherContext {
    temperature: number
    getTemperature?: () => Promise<any>
}

export const WeatherContext = React.createContext<IWeatherContext>({
    temperature: 0,
    getTemperature: async () => {},
})

export const WeatherProvider: React.FC<{children:any}> = props => {
    const [temperature, setTemperature] = React.useState(0)

    const getTemperature = React.useCallback(async () => {
        console.log("GETTEMP");
        try {
            const response = await axios({
                method: 'get',
                url: 'https://localhost:7173/WeatherForecast',
            })
                
            console.log("GETTEMP2");
            const temp = get(response, 'data[0].temperatureC')
            console.log("GETTEMP3");
            setTemperature(temp)
            return temp
        }
        catch (error) {
            console.log('error', error);
        }
    }, [setTemperature])

    return (
        <WeatherContext.Provider value={{ temperature, getTemperature }}>
            {props.children}
        </WeatherContext.Provider>
    )
}

export const useWeatherContext = () => React.useContext(WeatherContext)
