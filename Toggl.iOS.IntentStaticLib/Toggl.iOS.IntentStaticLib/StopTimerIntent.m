//
// StopTimerIntent.m
//
// This file was automatically generated and should not be edited.
//

#import "StopTimerIntent.h"

#if __has_include(<Intents/Intents.h>) && (!TARGET_OS_OSX || TARGET_OS_IOSMAC) && !TARGET_OS_TV

@implementation StopTimerIntent

@end

@interface StopTimerIntentResponse ()

@property (readwrite, NS_NONATOMIC_IOSONLY) StopTimerIntentResponseCode code;

@end

@implementation StopTimerIntentResponse

@synthesize code = _code;

@dynamic entryDescription, entryDurationString, entryStart, entryDuration;

- (instancetype)initWithCode:(StopTimerIntentResponseCode)code userActivity:(nullable NSUserActivity *)userActivity {
    self = [super init];
    if (self) {
        _code = code;
        self.userActivity = userActivity;
    }
    return self;
}

+ (instancetype)successIntentResponseWithEntryDescription:(NSString *)entryDescription entryDurationString:(NSString *)entryDurationString {
    StopTimerIntentResponse *intentResponse = [[StopTimerIntentResponse alloc] initWithCode:StopTimerIntentResponseCodeSuccess userActivity:nil];
    intentResponse.entryDescription = entryDescription;
    intentResponse.entryDurationString = entryDurationString;
    return intentResponse;
}

+ (instancetype)successWithEmptyDescriptionIntentResponseWithEntryDurationString:(NSString *)entryDurationString {
    StopTimerIntentResponse *intentResponse = [[StopTimerIntentResponse alloc] initWithCode:StopTimerIntentResponseCodeSuccessWithEmptyDescription userActivity:nil];
    intentResponse.entryDurationString = entryDurationString;
    return intentResponse;
}

@end

#endif
