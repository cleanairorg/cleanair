import {atom} from 'jotai';
import {AuthGetUserInfoDto, Devicelog, AdminChangesDeviceIntervalDto, ThresholdDto, ThresholdEvaluationResult} from "./generated-client.ts";

export const JwtAtom = atom<string>(localStorage.getItem('jwt') || '')

export const DeviceLogsAtom = atom<Devicelog[]>([]);

export const CurrentValueAtom = atom<Devicelog>();

export const UserInfoAtom = atom<AuthGetUserInfoDto | null>(null);

export const DeviceIntervalAtom = atom<AdminChangesDeviceIntervalDto | null>(null);

export const ThresholdsAtom = atom<ThresholdDto[]>([]);

export const EvaluationsAtom  = atom<ThresholdEvaluationResult[]>([]);