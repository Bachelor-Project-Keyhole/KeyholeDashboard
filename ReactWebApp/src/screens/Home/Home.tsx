import * as React from "react";
import { useWeatherContext } from "../../contexts/WeatherContext/WeatherContext";

const Home = ({...props}: any) => {

    const { temperature, getTemperature } = useWeatherContext()
    const [temp, setTemp] = React.useState(0)

     console.log("HI")

    const handleTemperature = React.useCallback(async () => {
      console.log("HANDLETEMP");
      try{
        let response = await getTemperature()
        console.log("RESPONSE", response);
      }
      catch (error) {
        console.log('error', error);
      }
    }
    , [getTemperature])

    handleTemperature()
   
  
  

  return (
    <div>{'Weather Temperature:'} + {temperature}  </div>
    
 );
}

export default Home;
