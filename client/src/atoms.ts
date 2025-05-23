import {atom} from 'jotai';
import {AuthGetUserInfoDto, Devicelog} from "./generated-client.ts";
import {ThresholdDto, ThresholdEvaluationResult} from "./generated-client.ts";

export const JwtAtom = atom<string>(localStorage.getItem('jwt') || '')

export const DeviceLogsAtom = atom<Devicelog[]>([]);

export const CurrentValueAtom = atom<Devicelog>();

export const UserInfoAtom = atom<AuthGetUserInfoDto | null>(null);

// To use for updated threshold ranges
export const ThresholdsAtom = atom<ThresholdDto[]>([]);

// To use for evaluation, got current device values and current status
export const ThresholdEvaluationsAtom = atom<ThresholdEvaluationResult[]>([]);