import {atom} from 'jotai';
import {AuthGetUserInfoDto, Devicelog, AdminChangesDeviceIntervalDto} from "./generated-client.ts";

export const JwtAtom = atom<string>(localStorage.getItem('jwt') || '')

export const DeviceLogsAtom = atom<Devicelog[]>([]);

export const CurrentValueAtom = atom<Devicelog>();

export const UserInfoAtom = atom<AuthGetUserInfoDto | null>(null);

export const DeviceIntervalAtom = atom<AdminChangesDeviceIntervalDto | null>(null);