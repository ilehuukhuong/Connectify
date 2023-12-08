import { InterestsDto } from "./interestsDto";
import { LookingForsDto } from "./lookingForsDto";
import { Photo } from "./photo";

export interface Member {
    id: number;
    userName: string;
    photoUrl: string;
    age: number;
    knownAs: string;
    created: Date;
    lastActive: Date;
    gender: string;
    introduction: string;
    lookingFors: LookingForsDto[];
    interests: InterestsDto[];
    city: string;
    photos: Photo[];    
    isVisible: boolean;
    fullName: string;
    distance: number;
}