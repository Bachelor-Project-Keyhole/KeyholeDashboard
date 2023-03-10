import * as React from 'react';
import * as ReactDOM from 'react-dom';
import { WeatherProvider } from './src/contexts/WeatherContext/WeatherContext';
import Home from './src/screens/Home';

function App() {
return (
    <WeatherProvider>
        <Home/>
    </WeatherProvider>

);
}
export default App;