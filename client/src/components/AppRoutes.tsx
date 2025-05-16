import {Route, Routes, useNavigate} from "react-router";
import useInitializeData from "../hooks/useInitializeData.tsx";
import {DashboardRoute, SignInRoute} from '../routeConstants.ts';
import useSubscribeToTopics from "../hooks/useSubscribeToTopics.tsx";
import {useEffect} from "react";
import {useAtom} from "jotai";
import {JwtAtom} from "../atoms.ts";
import toast from "react-hot-toast";
import Login from "./Login.tsx";
import Header from "./Header.tsx";
import DeviceSettings from "./DeviceSettings.tsx";
import CurrentValues from "./CurrentValues.tsx";
import MeasurementAlerts from "./MeasurementAlerts.tsx";
import Graphs from "./Graphs.tsx";

export default function AppRoutes(){

    const navigate = useNavigate();
    const [jwt] = useAtom(JwtAtom);
    useInitializeData();
    useSubscribeToTopics();

    useEffect(() => {
        if (jwt == undefined || jwt.length < 1) {
            navigate(SignInRoute)
            toast("Please sign in to continue")
        }
    }, [])

    const AppContent = (
        <div className="app">
            <Header />
            <main className="main-layout">
                <DeviceSettings />
                <CurrentValues />
                <MeasurementAlerts />
            </main>
            <Graphs />
        </div>
    );

    return (
        <>
            {/*the browser router is defined in main tsx so that i can use useNavigate anywhere*/}
            <Routes>

                <Route element={AppContent} path={DashboardRoute}/>
                <Route element={<Login/>} path={SignInRoute}/>

            </Routes>
        </>
    );
}