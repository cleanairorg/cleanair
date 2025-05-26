import {Navigate, Route, Routes, useNavigate} from "react-router";
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
import useThresholdBroadcastListener from "../hooks/useThresholdBroadcastListener";

export default function AppRoutes(){

    const navigate = useNavigate();
    const [jwt, setJwt] = useAtom(JwtAtom);
    useInitializeData();
    useSubscribeToTopics();
    useThresholdBroadcastListener();

    useEffect(() => {
        const storedJwt = localStorage.getItem("jwt");
        if (storedJwt) {
            setJwt(storedJwt);
            navigate(DashboardRoute);
        } else if (!jwt) {
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
                {/* Login route */}
                <Route path="/" element={<Login />} />
                <Route path={SignInRoute} element={<Login />} />

                {/* Protected dashboard route */}
                <Route path={DashboardRoute} element={jwt ? AppContent : <Navigate to={SignInRoute} replace />} />
            </Routes>
        </>
    );
}