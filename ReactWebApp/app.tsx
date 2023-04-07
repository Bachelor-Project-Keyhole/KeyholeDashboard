import * as React from 'react';
import { Link, Route, Routes } from 'react-router-dom';
import { WeatherProvider } from './src/contexts/WeatherContext/WeatherContext';
import Home from './src/screens/Home';
import Menu from './src/screens/Menu';

function App() {
return (
    <WeatherProvider>
        <nav>
            <ul>
                <li>
                    <Link to="/">Home</Link>
                </li>
                <li>
                    <Link to="/menu">Menu</Link>
                </li>
            </ul>
        </nav>
        <Routes>
            <Route path="/" element={<Home/>}/>
            <Route path="/menu" element={<Menu/>}/>
        </Routes>
        {/* <Home/> */}
        {/* <Menu/> */}
    </WeatherProvider>

);
}
export default App;