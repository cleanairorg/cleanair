import '../css/Login.css';
import '../css/App.css';
import {useNavigate} from "react-router";
import {DashboardRoute} from "../routeConstants.ts";

export default function Login(){

    const navigate = useNavigate();

    return (
        <>
            <div className="flex justify-center items-center h-screen app">
                <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs p-4 login-box">
                    <legend className="fieldset-legend text-white text-center text-2xl">Clean Air Login</legend>

                    <label className="label">Email</label>
                    <input type="email" className="input text-black" placeholder="Email"/>

                    <label className="label">Password</label>
                    <input type="password" className="input text-black" placeholder="Password"/>

                    <button className="btn btn-neutral mt-4" onClick={() => {navigate(DashboardRoute)}}>Login</button>
                </fieldset>
            </div>
        </>
    )
}