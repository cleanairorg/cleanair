import '../css/Header.css';
import {useAtom} from "jotai";
import {DeviceLogsAtom, JwtAtom, UserInfoAtom} from "../atoms.ts";
import {SignInRoute} from "../routeConstants.ts";
import {useNavigate} from "react-router";

export default function Header() {

    const navigate = useNavigate();
    const [, setJwt] = useAtom(JwtAtom);
    const [,setUserInfo] = useAtom(UserInfoAtom);
    const [, setDeviceLogs] = useAtom(DeviceLogsAtom);

    return (
        <header className="app header">
            <div className="header-left">
                <button className="app login-button" onClick={() => {
                    setJwt("");
                    setUserInfo(null);
                    setDeviceLogs([]);
                    localStorage.removeItem("jwt");
                    navigate(SignInRoute);
                }}>LOGOUT</button>
                {/*<button className="app register-button">REGISTER</button>*/}
            </div>
            <div className="header-center">
                <div className="app selected-device">SELECTED DEVICE ▼</div>
            </div>
            <div className="header-right">
                <div className="app logo">CleanAir</div>
            </div>
        </header>
    );
}
