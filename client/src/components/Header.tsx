import '../css/Header.css';
import {useAtom} from "jotai";
import {DeviceLogsAtom, JwtAtom, UserInfoAtom} from "../atoms.ts";
import {SignInRoute} from "../routeConstants.ts";
import {useNavigate} from "react-router";
import {subscriptionClient} from "../apiControllerClients.ts";
import {ChangeSubscriptionDto, StringConstants} from "../generated-client.ts";
import {randomUid} from "./App.tsx";
import toast from "react-hot-toast";

export default function Header() {

    const navigate = useNavigate();
    const [jwt, setJwt] = useAtom(JwtAtom);
    const [,setUserInfo] = useAtom(UserInfoAtom);
    const [, setDeviceLogs] = useAtom(DeviceLogsAtom);

    const subscribeDto: ChangeSubscriptionDto = {
        clientId: randomUid,
        topicIds: [StringConstants.Dashboard],
    };

    return (
        <header className="app header">
            <div className="header-left">
                <button className="app login-button" onClick={() => {
                    setJwt("");
                    setUserInfo(null);
                    setDeviceLogs([]);
                    localStorage.removeItem("jwt");
                    navigate(SignInRoute);
                    subscriptionClient.unsubscribe(
                        jwt, subscribeDto
                    ).then( r =>{
                        toast.success("Unsubscribed from dashboard topic");
                    })
                }}>LOGOUT</button>
                {/*<button className="app register-button">REGISTER</button>*/}
            </div>
            <div className="header-center">
                <div className="app selected-device">CLEANAIR DEVICE</div>
            </div>
            <div className="header-right">
                <div className="app logo">CleanAir</div>
            </div>
        </header>
    );
}
