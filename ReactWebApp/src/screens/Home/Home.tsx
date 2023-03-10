import * as React from "react";
import Button from "../../components/Button";
import Description from "../../components/Description";
import Header from "../../components/Header";
import PlusIcon from "../../components/PlusIcon";
import SubHeader from "../../components/SubHeader";
import Title from "../../components/Title";
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
    <>
      <div>{'Weather Temperature:'} + {temperature}  </div>
      <Title text="Home" />
      <Header text="Header" />
      <SubHeader text="SubHeader" />
      <Description text="Description Description Description Description Description Description Description Description" />
      <Button text="Button" icon={<PlusIcon/>} style={{backgroundColor: 'black', color: 'white'}} />
      <Button text="Button" icon={<PlusIcon color="red"/>} />
    </>
 );
}

export default Home;
