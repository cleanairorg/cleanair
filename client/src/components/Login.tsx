import '../css/Login.css';
import '../css/App.css';
import {useNavigate} from "react-router";
import {DashboardRoute} from "../routeConstants.ts";
import { useAtom } from 'jotai';
import {JwtAtom, UserInfoAtom} from '../atoms.ts';
import {authClient} from "../apiControllerClients.ts";
import toast from "react-hot-toast";
import {useState} from "react";
import * as React from "react";

export default function Login(){

    const navigate = useNavigate();

    const [jwt, setJwt] = useAtom(JwtAtom);
    const [userInfo, setUserInfo] = useAtom(UserInfoAtom);
    const [loginData, setLoginData] = useState({
        email: "",
        password: ""
    });

    return (
        <>
            <div className="flex justify-center items-center h-screen app">
                <fieldset className="fieldset bg-base-200 border-base-300 rounded-box w-xs p-4 login-box">
                    <legend className="fieldset-legend text-white text-center text-2xl">Clean Air Login</legend>

                    <label className="label">Email</label>
                    <input
                        type="email"
                        className="input text-black"
                        placeholder="Email"
                        onChange={(e) => {
                            setLoginData({...loginData, email: e.target.value})
                        }}
                    />

                    <label className="label">Password</label>
                    <input
                        type="password"
                        className="input text-black"
                        placeholder="Password"
                        onChange={(e) => {
                            setLoginData({...loginData, password: e.target.value})
                        }}
                    />
                    <button className="btn btn-neutral mt-4" onClick={() => {
                        authClient.login({email: loginData.email, password: loginData.password})
                            .then(r => {
                                if (r && r.jwt) {
                                    authClient.getUserInfo(loginData.email)
                                        .then(userInfo => {
                                            toast.success("Logged in successfully");
                                            setJwt(r.jwt);
                                            localStorage.setItem("jwt", r.jwt);
                                            setUserInfo(userInfo);
                                            navigate(DashboardRoute);
                                        })
                                        .catch(error => {
                                            console.error("Failed to fetch user info:", error);
                                            toast.error("Failed to fetch user information");
                                        });
                                } else {
                                    toast.error("Invalid login response");
                                }
                            })
                            .catch(error => {
                                console.error("Login failed:", error);
                                toast.error("Invalid email or password");
                            });
                    }}>Login
                    </button>
                    <button className="btn btn-neutral mt-4" onClick={() => {
                        authClient.register({email: loginData.email, password: loginData.password, role: "user"})
                            .then(r => {
                                if (r && r.jwt) {
                                    authClient.getUserInfo(loginData.email)
                                        .then(userInfo => {
                                            toast.success("Logged in successfully");
                                            setJwt(r.jwt);
                                            localStorage.setItem("jwt", r.jwt);
                                            setUserInfo(userInfo);
                                            navigate(DashboardRoute);
                                        })
                                        .catch(error => {
                                            console.error("Failed to fetch user info:", error);
                                            toast.error("Failed to fetch user information");
                                        });
                                } else {
                                    toast.error("Invalid login response");
                                }
                            })
                            .catch(error => {
                                console.error("Login failed:", error);
                                toast.error("Invalid email or password");
                            });
                    }}>Register a user
                    </button>

                    <button className="btn btn-neutral mt-4" onClick={() => {
                        authClient.register({email: loginData.email, password: loginData.password, role: "admin"})
                            .then(r => {
                                if (r && r.jwt) {
                                    authClient.getUserInfo(loginData.email)
                                        .then(userInfo => {
                                            toast.success("Logged in successfully");
                                            setJwt(r.jwt);
                                            localStorage.setItem("jwt", r.jwt);
                                            setUserInfo(userInfo);
                                            navigate(DashboardRoute);
                                        })
                                        .catch(error => {
                                            console.error("Failed to fetch user info:", error);
                                            toast.error("Failed to fetch user information");
                                        });
                                } else {
                                    toast.error("Invalid login response");
                                }
                            })
                            .catch(error => {
                                console.error("Login failed:", error);
                                toast.error("Invalid email or password");
                            });
                    }}>Register a admin
                    </button>
                </fieldset>
            </div>
        </>
    )
}