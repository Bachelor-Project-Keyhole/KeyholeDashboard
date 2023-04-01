import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { WeatherProvider } from './src/contexts/WeatherContext/WeatherContext';
import Home from './src/screens/Home';
import Menu from './src/screens/Menu';

function App() {
return (
    <WeatherProvider>
        {/* <Home/> */}
        <Menu/>
    </WeatherProvider>

);
}
export default App;