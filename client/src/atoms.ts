import {atom} from 'jotai';
import {AuthGetUserInfoDto, Devicelog} from "./generated-client.ts";

export const JwtAtom = atom<string>(localStorage.getItem('jwt') || '')

export const DeviceLogsAtom = atom<Devicelog[]>([]);

export const CurrentValueAtom = atom<Devicelog>();

export const UserInfoAtom = atom<AuthGetUserInfoDto | null>(null);