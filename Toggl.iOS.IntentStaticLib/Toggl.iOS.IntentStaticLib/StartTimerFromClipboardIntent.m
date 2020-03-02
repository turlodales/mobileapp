//
// StartTimerFromClipboardIntent.m
//
// This file was automatically generated and should not be edited.
//

#import "StartTimerFromClipboardIntent.h"

#if __has_include(<Intents/Intents.h>) && (!TARGET_OS_OSX || TARGET_OS_IOSMAC) && !TARGET_OS_TV

@implementation StartTimerFromClipboardIntent

@dynamic workspace, billable, projectId, taskId, tags;

@end

@interface StartTimerFromClipboardIntentResponse ()

@property (readwrite, NS_NONATOMIC_IOSONLY) StartTimerFromClipboardIntentResponseCode code;

@end

@implementation StartTimerFromClipboardIntentResponse

@synthesize code = _code;

- (instancetype)initWithCode:(StartTimerFromClipboardIntentResponseCode)code userActivity:(nullable NSUserActivity *)userActivity {
    self = [super init];
    if (self) {
        _code = code;
        self.userActivity = userActivity;
    }
    return self;
}

@end

#endif
