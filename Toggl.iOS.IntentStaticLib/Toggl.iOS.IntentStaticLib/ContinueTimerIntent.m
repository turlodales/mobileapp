//
// ContinueTimerIntent.m
//
// This file was automatically generated and should not be edited.
//

#import "ContinueTimerIntent.h"

#if __has_include(<Intents/Intents.h>) && (!TARGET_OS_OSX || TARGET_OS_IOSMAC) && !TARGET_OS_TV

@implementation ContinueTimerIntent

@end

@interface ContinueTimerIntentResponse ()

@property (readwrite, NS_NONATOMIC_IOSONLY) ContinueTimerIntentResponseCode code;

@end

@implementation ContinueTimerIntentResponse

@synthesize code = _code;

@dynamic entryDescription;

- (instancetype)initWithCode:(ContinueTimerIntentResponseCode)code userActivity:(nullable NSUserActivity *)userActivity {
    self = [super init];
    if (self) {
        _code = code;
        self.userActivity = userActivity;
    }
    return self;
}

+ (instancetype)successWithEntryDescriptionIntentResponseWithEntryDescription:(NSString *)entryDescription {
    ContinueTimerIntentResponse *intentResponse = [[ContinueTimerIntentResponse alloc] initWithCode:ContinueTimerIntentResponseCodeSuccessWithEntryDescription userActivity:nil];
    intentResponse.entryDescription = entryDescription;
    return intentResponse;
}

@end

#endif
